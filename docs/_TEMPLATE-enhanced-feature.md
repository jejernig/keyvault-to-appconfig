# Feature: [Feature Name]

## Summary
- **Persona / User**: [Primary user role]
- **Need**: [Clear problem statement]
- **Outcome**: [Measurable business outcome]

## Implementation Guidance

- **Phase**: Phase [0/1/2] ([Foundation/MVP/Scale])
- **Epic Dependency**: [Epic ID] — [Epic Name]
- **Build Order**: [Number]
- **Code Touchpoints**: [packages/services affected]
- **Tests**: Unit + integration + [performance/security/e2e]
- **Implementation Scope**: [High-level scope statement]

## PDCA (Build Plan)

- **Plan**: [What needs to be designed/architected]
- **Do**: [What needs to be implemented]
- **Check**: [How to validate it works]
- **Act**: [How to iterate based on feedback]

## Experience Narrative
1. [Step-by-step user journey with system interactions]
2. [Include happy path and key decision points]
3. [Show data flow and integrations]

## Acceptance Criteria
- [ ] Given [precondition], when [action], then [expected outcome].
- [ ] Given [precondition], when [action], then [expected outcome].
- [ ] [Continue with specific, testable criteria]

---

## Technical Specification

### Data Models

#### PostgreSQL Schema (Supabase + Prisma)

```prisma
// Primary entity
model EntityName {
  id          String   @id @default(uuid())
  tenantId    String   @db.Uuid // REQUIRED for multi-tenancy

  // Business fields
  name        String   @db.VarChar(255)
  status      Status   @default(ACTIVE)

  // Relationships
  ownerId     String   @db.Uuid
  owner       User     @relation(fields: [ownerId], references: [id])

  // Audit fields (standard)
  createdAt   DateTime @default(now())
  createdBy   String   @db.Uuid
  updatedAt   DateTime @updatedAt
  updatedBy   String?  @db.Uuid
  deletedAt   DateTime? // Soft delete

  // Indexes
  @@index([tenantId, status]) // Always index tenantId first
  @@index([ownerId])
  @@unique([tenantId, name]) // Example unique constraint

  // RLS - See Row-Level Security section
}

enum Status {
  ACTIVE
  INACTIVE
  ARCHIVED
}

// Supporting types
type BusinessLogicType = {
  field1: string;
  field2: number;
  // Include validation constraints in comments
}
```

#### Neo4j Relationship Schema

```cypher
// Node definitions
(:EntityName {
  id: UUID,
  tenantId: UUID,
  name: String,
  createdAt: DateTime
})

// Relationship patterns
(:User)-[:OWNS {
  since: DateTime,
  permissions: [String]
}]->(:EntityName)

(:EntityName)-[:RELATED_TO {
  type: String,
  strength: Float
}]->(:OtherEntity)

// Indexes (create on application startup)
CREATE INDEX entity_id_index IF NOT EXISTS FOR (e:EntityName) ON (e.id);
CREATE INDEX entity_tenant_index IF NOT EXISTS FOR (e:EntityName) ON (e.tenantId);
```

#### Redis Cache Schema

```typescript
// Cache key naming convention
type CacheKeys = {
  // Single entity: {prefix}:{entity}:{id}
  entity: (id: string) => `athleet:entity:${id}`,

  // List/query: {prefix}:{entity}:list:{filter}:{page}
  entityList: (tenantId: string, status: string, page: number) =>
    `athleet:entity:list:${tenantId}:${status}:${page}`,

  // Relationships: {prefix}:{entity}:{id}:{relation}
  entityRelations: (id: string) => `athleet:entity:${id}:relations`,
};

// TTL Strategy
const CACHE_TTL = {
  entity: 3600,        // 1 hour for single entities
  entityList: 300,     // 5 min for lists (more volatile)
  entityRelations: 1800, // 30 min for relationships
};

// Cache invalidation triggers
// - On entity create/update/delete: invalidate entity:{id}
// - On entity update: invalidate entityList:* for that tenant
// - On relationship change: invalidate entityRelations:{id}
```

### API Specification

#### REST Endpoints

```typescript
// GET /api/entities/:id
type GetEntityRequest = {
  params: { id: string };
  headers: { authorization: string }; // Required
};

type GetEntityResponse = {
  success: true;
  data: {
    id: string;
    name: string;
    status: Status;
    owner: { id: string; name: string };
    createdAt: string;
    updatedAt: string;
  };
} | ErrorResponse;

// Status Codes:
// 200 - Success
// 401 - Unauthorized (no token)
// 403 - Forbidden (wrong tenant)
// 404 - Entity not found
// 500 - Internal server error

// POST /api/entities
type CreateEntityRequest = {
  body: {
    name: string;      // Required, 1-255 chars
    status?: Status;   // Optional, defaults to ACTIVE
    metadata?: object; // Optional JSON
  };
  headers: { authorization: string };
};

type CreateEntityResponse = {
  success: true;
  data: {
    id: string;
    name: string;
    status: Status;
    createdAt: string;
  };
} | ErrorResponse;

// Status Codes:
// 201 - Created
// 400 - Validation error
// 401 - Unauthorized
// 409 - Conflict (duplicate name)
// 500 - Internal server error

// PATCH /api/entities/:id
// DELETE /api/entities/:id
// [Additional endpoints...]
```

#### Validation Rules

```typescript
const EntityValidation = {
  name: {
    required: true,
    minLength: 1,
    maxLength: 255,
    pattern: /^[a-zA-Z0-9\s-]+$/, // alphanumeric, spaces, hyphens
    errorMessage: 'Name must be 1-255 alphanumeric characters',
  },
  status: {
    required: false,
    enum: ['ACTIVE', 'INACTIVE', 'ARCHIVED'],
    errorMessage: 'Status must be one of: ACTIVE, INACTIVE, ARCHIVED',
  },
  metadata: {
    required: false,
    type: 'object',
    maxSize: 10000, // bytes
    errorMessage: 'Metadata must be valid JSON under 10KB',
  },
};

// Input sanitization
function sanitizeEntityInput(input: unknown): CreateEntityRequest['body'] {
  // XSS prevention
  const sanitized = {
    name: DOMPurify.sanitize(String(input.name)).trim(),
    status: input.status ? String(input.status).toUpperCase() : undefined,
    metadata: input.metadata ? sanitizeJSON(input.metadata) : undefined,
  };

  // Validation
  const errors = validateEntity(sanitized);
  if (errors.length > 0) {
    throw new ValidationError(errors);
  }

  return sanitized;
}
```

### Error Handling

#### Error Codes

```typescript
enum ErrorCode {
  // Validation Errors (4000-4099)
  VALIDATION_FAILED = 'ERR_4000',
  INVALID_NAME = 'ERR_4001',
  INVALID_STATUS = 'ERR_4002',

  // Authorization Errors (4100-4199)
  UNAUTHORIZED = 'ERR_4100',
  FORBIDDEN = 'ERR_4101',
  INSUFFICIENT_PERMISSIONS = 'ERR_4102',

  // Resource Errors (4400-4499)
  NOT_FOUND = 'ERR_4404',
  ALREADY_EXISTS = 'ERR_4409',

  // Business Logic Errors (4500-4599)
  BUSINESS_RULE_VIOLATION = 'ERR_4500',
  INVALID_STATE_TRANSITION = 'ERR_4501',

  // System Errors (5000-5099)
  INTERNAL_ERROR = 'ERR_5000',
  DATABASE_ERROR = 'ERR_5001',
  CACHE_ERROR = 'ERR_5002',
  EXTERNAL_SERVICE_ERROR = 'ERR_5003',
}

type ErrorResponse = {
  success: false;
  error: {
    code: ErrorCode;
    message: string;
    details?: Record<string, string[]>; // field-level errors
    retryable: boolean;
    timestamp: string;
    correlationId: string;
  };
};

// Error handling strategy
async function handleOperation(): Promise<Result> {
  try {
    // Attempt operation
    return await performOperation();
  } catch (error) {
    // Log error with context
    logger.error('Operation failed', {
      error,
      correlationId: req.correlationId,
      tenantId: req.tenantId,
      userId: req.userId,
    });

    // Map to API error
    if (error instanceof ValidationError) {
      return {
        success: false,
        error: {
          code: ErrorCode.VALIDATION_FAILED,
          message: 'Validation failed',
          details: error.fieldErrors,
          retryable: false,
          timestamp: new Date().toISOString(),
          correlationId: req.correlationId,
        },
      };
    }

    // Generic error response
    return {
      success: false,
      error: {
        code: ErrorCode.INTERNAL_ERROR,
        message: 'An unexpected error occurred',
        retryable: true,
        timestamp: new Date().toISOString(),
        correlationId: req.correlationId,
      },
    };
  }
}
```

#### Retry Strategy

```typescript
// Retry eligibility by error type
const RETRYABLE_ERRORS = [
  ErrorCode.DATABASE_ERROR,
  ErrorCode.CACHE_ERROR,
  ErrorCode.EXTERNAL_SERVICE_ERROR,
];

// Exponential backoff configuration
const RETRY_CONFIG = {
  maxRetries: 3,
  baseDelayMs: 100,
  maxDelayMs: 2000,
  jitterMs: 50,
};

async function withRetry<T>(
  operation: () => Promise<T>,
  config = RETRY_CONFIG
): Promise<T> {
  let lastError: Error;

  for (let attempt = 0; attempt <= config.maxRetries; attempt++) {
    try {
      return await operation();
    } catch (error) {
      lastError = error;

      // Don't retry if not retryable or last attempt
      if (!isRetryable(error) || attempt === config.maxRetries) {
        throw error;
      }

      // Calculate delay with exponential backoff + jitter
      const delay = Math.min(
        config.baseDelayMs * Math.pow(2, attempt) + Math.random() * config.jitterMs,
        config.maxDelayMs
      );

      await sleep(delay);
    }
  }

  throw lastError!;
}
```

### Multi-Tenancy & Data Isolation

#### Row-Level Security (RLS) Policies

```sql
-- Enable RLS on table
ALTER TABLE entities ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only see entities in their tenant
CREATE POLICY tenant_isolation_policy ON entities
  FOR ALL
  USING (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- Policy: Users can only insert entities for their tenant
CREATE POLICY tenant_insert_policy ON entities
  FOR INSERT
  WITH CHECK (tenant_id = current_setting('app.current_tenant_id')::uuid);

-- Policy: Users can only update their own tenant's entities
CREATE POLICY tenant_update_policy ON entities
  FOR UPDATE
  USING (tenant_id = current_setting('app.current_tenant_id')::uuid)
  WITH CHECK (tenant_id = current_setting('app.current_tenant_id')::uuid);
```

#### Tenant Context Propagation

```typescript
// Middleware to set tenant context
export async function tenantContext(
  req: Request,
  res: Response,
  next: NextFunction
) {
  // Extract tenant from JWT
  const tenantId = req.user?.tenantId;

  if (!tenantId) {
    return res.status(401).json({
      success: false,
      error: {
        code: ErrorCode.UNAUTHORIZED,
        message: 'Tenant context required',
        retryable: false,
      },
    });
  }

  // Set Prisma tenant context
  await prisma.$executeRaw`
    SELECT set_config('app.current_tenant_id', ${tenantId}, true)
  `;

  // Add to request context
  req.tenantId = tenantId;

  next();
}

// Usage in queries
async function getEntities(tenantId: string) {
  // RLS automatically filters by tenant
  return await prisma.entity.findMany({
    // No need to add: where: { tenantId }
    // RLS handles this automatically
  });
}
```

#### Cross-Tenant Access Prevention

```typescript
// Validation: Ensure referenced entities belong to same tenant
async function validateTenantOwnership(
  tenantId: string,
  entityId: string
): Promise<void> {
  const entity = await prisma.entity.findUnique({
    where: { id: entityId },
    select: { tenantId: true },
  });

  if (!entity || entity.tenantId !== tenantId) {
    throw new ForbiddenError('Entity not found or access denied');
  }
}
```

### Transaction Management

```typescript
// Transaction pattern for multi-table operations
async function createEntityWithRelations(data: CreateEntityData) {
  return await prisma.$transaction(async (tx) => {
    // 1. Create main entity
    const entity = await tx.entity.create({
      data: {
        name: data.name,
        tenantId: data.tenantId,
        ownerId: data.ownerId,
      },
    });

    // 2. Create Neo4j relationship (saga pattern - see below if this fails)
    await createNeo4jRelationship(entity.id, data.ownerId);

    // 3. Create related records
    await tx.entityMetadata.create({
      data: {
        entityId: entity.id,
        metadata: data.metadata,
      },
    });

    // 4. Invalidate cache
    await cache.delete(`entity:list:${data.tenantId}:*`);

    return entity;
  }, {
    isolationLevel: 'ReadCommitted', // or 'Serializable' for stronger consistency
    timeout: 5000, // 5 second timeout
  });
}

// Saga pattern for cross-database consistency
async function createEntityWithGraph(data: CreateEntityData) {
  let entityId: string | null = null;

  try {
    // Step 1: Create in PostgreSQL
    const entity = await prisma.entity.create({ data });
    entityId = entity.id;

    // Step 2: Create in Neo4j
    await createNeo4jNode(entity);

    return entity;
  } catch (error) {
    // Log saga failure with structured metadata (never use console.error)
    logger.error('Saga transaction failed during entity creation', {
      entityId,
      error,
      compensationAttempted: !!entityId,
      tenantId: data.tenantId,
      timestamp: new Date().toISOString(),
    });

    // Compensate: rollback PostgreSQL if Neo4j fails
    if (entityId) {
      await prisma.entity.delete({ where: { id: entityId } }).catch((deleteError) => {
        // Log compensation failure with structured metadata (never use console.error)
        logger.error('Failed to rollback entity during saga compensation', {
          entityId,
          originalError: error,
          deleteError,
          tenantId: data.tenantId,
          timestamp: new Date().toISOString(),
        });
      });
    }
    throw error;
  }
}
```

### Observability & Monitoring

#### Metrics

```typescript
// Key performance indicators
const metrics = {
  // Latency metrics (milliseconds)
  'entity.create.latency': histogram({
    description: 'Entity creation latency',
    buckets: [10, 50, 100, 200, 500, 1000, 2000],
  }),

  // Throughput metrics (operations per second)
  'entity.create.count': counter({
    description: 'Total entity creations',
    labels: ['tenant_id', 'status'],
  }),

  // Error metrics
  'entity.create.errors': counter({
    description: 'Entity creation errors',
    labels: ['error_code', 'retryable'],
  }),

  // Cache metrics
  'entity.cache.hit_rate': gauge({
    description: 'Entity cache hit rate',
    labels: ['cache_key_type'],
  }),
};

// Instrumentation
async function createEntity(data: CreateEntityData) {
  const startTime = Date.now();

  try {
    const entity = await performCreate(data);

    // Record success metrics
    metrics['entity.create.latency'].observe(Date.now() - startTime);
    metrics['entity.create.count'].inc({
      tenant_id: data.tenantId,
      status: 'success',
    });

    return entity;
  } catch (error) {
    // Record error metrics
    metrics['entity.create.errors'].inc({
      error_code: error.code,
      retryable: String(error.retryable),
    });

    throw error;
  }
}
```

#### Logging

```typescript
// Structured logging with correlation
logger.info('Entity created', {
  // Correlation
  correlationId: req.correlationId,
  traceId: req.traceId,

  // Context
  tenantId: req.tenantId,
  userId: req.userId,

  // Entity
  entityId: entity.id,
  entityName: entity.name,

  // Performance
  durationMs: Date.now() - startTime,

  // Metadata
  timestamp: new Date().toISOString(),
  service: 'api',
  action: 'entity.create',
});

// Error logging
logger.error('Entity creation failed', {
  correlationId: req.correlationId,
  tenantId: req.tenantId,
  userId: req.userId,
  error: {
    code: error.code,
    message: error.message,
    stack: error.stack,
  },
  input: sanitizeForLogging(data), // Remove PII
  timestamp: new Date().toISOString(),
});
```

#### Service Level Objectives (SLOs)

```typescript
// Performance targets
const SLO = {
  // Latency (95th percentile)
  'entity.create.latency.p95': 200,  // 200ms
  'entity.read.latency.p95': 50,     // 50ms
  'entity.list.latency.p95': 100,    // 100ms

  // Throughput
  'entity.create.throughput': 100,   // 100 req/sec

  // Availability
  'entity.api.uptime': 99.9,         // 99.9% uptime

  // Error rate
  'entity.api.error_rate': 0.1,      // <0.1% error rate
};

// Alert conditions
const ALERTS = {
  'entity.latency.high': {
    condition: 'p95_latency > 500ms for 5 minutes',
    severity: 'warning',
    action: 'Investigate performance degradation',
  },
  'entity.error_rate.high': {
    condition: 'error_rate > 1% for 2 minutes',
    severity: 'critical',
    action: 'Page on-call engineer',
  },
  'entity.cache.hit_rate.low': {
    condition: 'cache_hit_rate < 50% for 10 minutes',
    severity: 'warning',
    action: 'Review cache configuration',
  },
};
```

### Security & Authorization

#### Permission Requirements

```typescript
// RBAC permissions for this feature
enum EntityPermission {
  CREATE = 'entity:create',
  READ = 'entity:read',
  UPDATE = 'entity:update',
  DELETE = 'entity:delete',
  LIST = 'entity:list',
  ADMIN = 'entity:admin', // Can manage all entities in tenant
}

// Permission middleware
async function requirePermission(permission: EntityPermission) {
  return async (req: Request, res: Response, next: NextFunction) => {
    const userPermissions = req.user?.permissions || [];

    if (!userPermissions.includes(permission)) {
      return res.status(403).json({
        success: false,
        error: {
          code: ErrorCode.INSUFFICIENT_PERMISSIONS,
          message: `Required permission: ${permission}`,
          retryable: false,
        },
      });
    }

    next();
  };
}

// Usage
app.post('/api/entities',
  authenticate,
  requirePermission(EntityPermission.CREATE),
  createEntity
);
```

#### Input Sanitization

```typescript
// XSS prevention
import DOMPurify from 'isomorphic-dompurify';

function sanitizeInput(input: unknown): SanitizedInput {
  if (typeof input === 'string') {
    return DOMPurify.sanitize(input.trim());
  }

  if (typeof input === 'object' && input !== null) {
    const sanitized: any = {};
    for (const [key, value] of Object.entries(input)) {
      sanitized[key] = sanitizeInput(value);
    }
    return sanitized;
  }

  return input;
}

// SQL injection prevention (Prisma handles this automatically)
// Never use raw SQL with user input unless parameterized
```

#### Rate Limiting

```typescript
// Rate limit configuration
const RATE_LIMITS = {
  'entity.create': {
    windowMs: 60000,    // 1 minute
    maxRequests: 100,   // 100 requests per window
    keyGenerator: (req) => `${req.tenantId}:${req.userId}`,
  },
  'entity.list': {
    windowMs: 60000,
    maxRequests: 1000,  // Higher limit for reads
    keyGenerator: (req) => req.tenantId,
  },
};

// Apply rate limiting
app.post('/api/entities',
  rateLimit(RATE_LIMITS['entity.create']),
  createEntity
);
```

### Testing Strategy

#### Unit Tests

```typescript
describe('Entity Creation', () => {
  it('should create entity with valid data', async () => {
    const data = {
      name: 'Test Entity',
      tenantId: 'tenant-123',
      ownerId: 'user-123',
    };

    const entity = await createEntity(data);

    expect(entity.id).toBeDefined();
    expect(entity.name).toBe('Test Entity');
    expect(entity.tenantId).toBe('tenant-123');
  });

  it('should reject invalid name', async () => {
    const data = {
      name: '', // Invalid
      tenantId: 'tenant-123',
      ownerId: 'user-123',
    };

    await expect(createEntity(data)).rejects.toThrow(ValidationError);
  });

  it('should enforce tenant isolation', async () => {
    // Create entity in tenant A
    const entity = await createEntity({ tenantId: 'tenant-a', ... });

    // Try to access from tenant B
    await expect(
      getEntity(entity.id, { tenantId: 'tenant-b' })
    ).rejects.toThrow(ForbiddenError);
  });
});
```

#### Integration Tests

```typescript
describe('Entity API Integration', () => {
  it('should create entity via API', async () => {
    const response = await request(app)
      .post('/api/entities')
      .set('Authorization', `Bearer ${authToken}`)
      .send({ name: 'Test Entity' });

    expect(response.status).toBe(201);
    expect(response.body.success).toBe(true);
    expect(response.body.data.id).toBeDefined();
  });

  it('should cache entity after creation', async () => {
    const entity = await createEntity(data);

    // Verify cache
    const cached = await cache.get(`entity:${entity.id}`);
    expect(cached).toEqual(entity);
  });

  it('should create Neo4j relationship', async () => {
    const entity = await createEntity(data);

    // Verify graph
    const relationship = await neo4j.run(
      'MATCH (u:User {id: $userId})-[r:OWNS]->(e:Entity {id: $entityId}) RETURN r',
      { userId: data.ownerId, entityId: entity.id }
    );

    expect(relationship.records).toHaveLength(1);
  });
});
```

#### Performance Tests

```typescript
describe('Entity Performance', () => {
  it('should create entity within SLO', async () => {
    const startTime = Date.now();
    await createEntity(data);
    const duration = Date.now() - startTime;

    expect(duration).toBeLessThan(SLO['entity.create.latency.p95']);
  });

  it('should handle concurrent creates', async () => {
    const promises = Array(100).fill(null).map((_, i) =>
      createEntity({ name: `Entity ${i}`, ...commonData })
    );

    const results = await Promise.all(promises);
    expect(results).toHaveLength(100);
    expect(new Set(results.map(r => r.id)).size).toBe(100); // All unique
  });

  it('should list entities within SLO', async () => {
    // Create 1000 entities
    await seedEntities(1000);

    const startTime = Date.now();
    const entities = await listEntities({ page: 1, limit: 20 });
    const duration = Date.now() - startTime;

    expect(duration).toBeLessThan(SLO['entity.list.latency.p95']);
    expect(entities).toHaveLength(20);
  });
});
```

#### Test Data Management

```typescript
// Test data factory
const EntityFactory = {
  build: (overrides = {}) => ({
    name: faker.commerce.productName(),
    tenantId: 'test-tenant',
    ownerId: 'test-user',
    status: 'ACTIVE',
    ...overrides,
  }),

  create: async (overrides = {}) => {
    const data = EntityFactory.build(overrides);
    return await prisma.entity.create({ data });
  },

  createMany: async (count: number, overrides = {}) => {
    const entities = Array(count).fill(null).map(() =>
      EntityFactory.build(overrides)
    );
    return await prisma.entity.createMany({ data: entities });
  },
};

// Test cleanup
afterEach(async () => {
  await prisma.entity.deleteMany({ where: { tenantId: 'test-tenant' } });
  await neo4j.run('MATCH (e:Entity {tenantId: "test-tenant"}) DETACH DELETE e');
  await cache.deletePattern('athleet:entity:*');
});
```

---

## Dependencies
- [List feature dependencies with specific integration points]
- [Include both build order dependencies and runtime dependencies]

## Research Notes
- [Technical decisions and rationale]
- [Alternative approaches considered]
- [Performance considerations]
- [Scalability considerations]

## Launch Readiness
- QA Owner: [Team/Role]
- Analytics: [List of metrics to track post-launch]
- Documentation: [List of docs to create]

## Implementation Checklist

- [ ] Data models defined in Prisma schema
- [ ] Neo4j indexes and relationships created
- [ ] RLS policies applied
- [ ] API endpoints implemented with OpenAPI docs
- [ ] Validation rules implemented
- [ ] Error codes defined and error handling added
- [ ] Retry logic implemented for external calls
- [ ] Caching layer implemented with invalidation
- [ ] Tenant context middleware applied
- [ ] Transaction boundaries defined
- [ ] Metrics instrumentation added
- [ ] Logging with correlation IDs
- [ ] SLO/alert thresholds configured
- [ ] Permission checks implemented
- [ ] Rate limiting configured
- [ ] Input sanitization applied
- [ ] Unit tests written (>80% coverage)
- [ ] Integration tests written
- [ ] Performance tests written and passing SLOs
- [ ] Security review completed
- [ ] Documentation updated
