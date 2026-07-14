export interface Company {
  id: string;
  name: string;
  taxId: string;
  logoUrl?: string;
  country: string;
}

export interface UserProfile {
  sub: string;
  email: string;
  name: string;
  role: string[];
  avatarUrl?: string;
  tenantId: string;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  type: 'Info' | 'Warning' | 'Error' | 'Success' | 'AI';
  category: string;
  actionUrl?: string;
  createdAt: string;
}

export interface InboxItem {
  id: string;
  companyId: string;
  companyName: string;
  type: string;
  status: string;
  aiPriority: 'Critical' | 'High' | 'Medium' | 'Low';
  description: string;
  dueDate?: string;
  createdAt: string;
}

export interface KpiData {
  label: string;
  value: number | string;
  unit?: string;
  trend?: number;
  icon: string;
  route?: string;
}

export interface AuditEvent {
  id: string;
  entityName: string;
  entityType: string;
  entityId: string;
  action: string;
  userId: string;
  actor: string;
  actorType: 'User' | 'Agent';
  details?: Record<string, unknown>;
  changes?: Record<string, unknown>;
  reasoningTrace?: ReasoningStep[];
  timestamp: string;
  occurredAt: string;
}

export interface ReasoningStep {
  stepNumber: number;
  stepType: string;
  description: string;
  inputData?: string;
  outputData?: string;
  confidenceScore?: number;
  durationMs: number;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  isStreaming?: boolean;
}

export interface ReportSection {
  label: string;
  amount: number;
  children?: ReportSection[];
  isExpanded?: boolean;
}

export interface ReportData {
  title: string;
  period: string;
  sections: ReportSection[];
  generatedAt: string;
}

export interface AccountNode {
  code: string;
  name: string;
  type: 'Asset' | 'Liability' | 'Equity' | 'Revenue' | 'Expense';
  balance: number;
  children?: AccountNode[];
  isExpanded?: boolean;
}

export interface BankTransaction {
  id: string;
  date: string;
  description: string;
  amount: number;
  reference?: string;
  status: 'Imported' | 'Matched' | 'Reconciled' | 'Discarded';
}

export interface ReconciliationMatch {
  id: string;
  bankTransactionId: string;
  bookEntryId: string;
  confidence: number;
  status: 'Pending' | 'Approved' | 'Rejected';
}

export interface JournalEntry {
  id: string;
  date: string;
  description: string;
  lines: JournalEntryLine[];
  sourceDocumentId?: string;
  aiReasoningTraceId?: string;
  status: 'Draft' | 'Pending' | 'Approved' | 'Rejected';
}

export interface JournalEntryLine {
  accountCode: string;
  accountName: string;
  debit: number;
  credit: number;
  description?: string;
}

export interface UserPreferences {
  theme: 'light' | 'dark' | 'system';
  language: 'es' | 'en';
  currency: string;
  dateFormat: string;
  fiscalYearStart: number;
  notificationsEnabled: boolean;
  emailNotifications: boolean;
  pushNotifications: boolean;
  dailySummaryEmail: boolean;
  aiConfidenceThreshold: number;
  autoApproveHighConfidence: boolean;
  sidebarCollapsed: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface UserDto {
  id: string;
  name: string;
  lastName: string;
  middleName?: string;
  userName: string;
  email: string;
  identification: string;
  identificationTypeId: string;
  tenantId: string;
  securityUserId: string;
  roles: UserRoleDto[];
}

export interface UserRoleDto {
  roleId: string;
  roleName: string;
}

export interface CreateUserCommand {
  name: string;
  middleName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
  identification: string;
  identificationTypeId: string;
  tenantId: string;
}

export interface UpdateUserCommand {
  userId: string;
  name: string;
  middleName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
  identification: string;
  identificationTypeId: string;
  tenantId: string;
}

export interface RoleDto {
  id: string;
  isActive: boolean;
  name: string;
  claims: RolePermissionDto[];
}

export interface RolePermissionDto {
  permissionId: string;
  permissionName: string;
  permissionResource: string;
  permissionAction: string;
}

export interface CreateRoleCommand {
  roleName: string;
}

export interface UpdateRoleCommand {
  id: string;
  roleName: string;
}

export interface PermissionDto {
  permissionId: string;
  provider: string;
  description?: string;
  action: string;
  resource: string;
  isSystem: boolean;
}

export interface CreatePermissionCommand {
  permissionId: string;
  provider: string;
  description?: string;
  action: string;
  resource: string;
  isSystem: boolean;
}

export interface UpdatePermissionCommand {
  permissionId: string;
  provider: string;
  description?: string;
  action: string;
  resource: string;
  isSystem: boolean;
}

export interface ApplicationDto {
  clientId: string;
  displayName?: string;
  clientType?: string;
  applicationType?: string;
  clientSecret?: string;
  consentType?: string;
  permissions: string[];
  requirements: string[];
  redirectUris: string[];
  postLogoutRedirectUris: string[];
}

export interface ScopeDto {
  name: string;
  resources: string[];
}

export interface CreateScopeCommand {
  name: string;
  resources: string[];
}

export interface UpdateScopeCommand {
  name: string;
  resources: string[];
}

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  createdAtUtc: string;
  userCount: number;
}

export interface IdentificationTypeDto {
  id: string;
  name: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface ApiEnvelope<T> {
  data: T;
  statusCode: number;
  message: string;
  errors: string[];
}
