import { useAuthStore } from "stores/auth";

// Permission catalogue keys (mirror Sakolee.Shared.Security.Permissions). Centralised so
// components reference a constant rather than scattering magic strings.
export const Permissions = Object.freeze({
  TenantsRead: "tenants.read",
  TenantsWrite: "tenants.write",
  TenantsArchive: "tenants.archive",
  PersonsRead: "persons.read",
  PersonsWrite: "persons.write",
  PersonsDelete: "persons.delete",
  StudentsRead: "students.read",
  StudentsWrite: "students.write",
  StudentsDelete: "students.delete",
  ClassesRead: "classes.read",
  ClassesWrite: "classes.write",
  ClassesDelete: "classes.delete",
  LocationsRead: "locations.read",
  LocationsWrite: "locations.write",
  LocationsDelete: "locations.delete",

  // Family Statuses Permissions
  FamilyStatusesRead: "familyStatuses.read",
  FamilyStatusesWrite: "familyStatuses.write",
  FamilyStatusesDelete: "familyStatuses.delete",
  // Class Sessions Permissions
  ClassSessionsRead: "classSessions.read",
  ClassSessionsWrite: "classSessions.write",
  ClassSessionsDelete: "classSessions.delete",
  // Bank Methods Permissions
  BillingMethodsRead: "billingMethods.read",
  BillingMethodsWrite: "billingMethods.write",
  BillingMethodsDelete: "billingMethods.delete",

  //Family Relations Permissions
  FamilyRelationsRead : " familyRelations.read",
  FamilyRelationsWrite : "familyRelations.write",
  FamilyRelationsDelete : "familyRelations.delete",

  UsersRead: "users.read",
  UsersWrite: "users.write",
  UsersResetPassword: "users.reset_password",
  UsersGroupManagement: "users.groupManagement",
  RolesRead: "roles.read",
  RolesWrite: "roles.write",
  RolesAssign: "roles.assign",
  GroupsManage: "groups.manage",
  EmailManage: "email.manage",
  // Universal Features (Phase 14/15).
  SettingsManage: "settings.manage",
  RecordsAdminDelete: "records.adminDelete",
  // Option Sets (tenant-configurable input value lists).
  OptionSetsRead: "optionSets.read",
  OptionSetsManage: "optionSets.manage"
});

// Reactive permission checks for the active tenant. `has`/`hasAny` read the auth store's
// permissions (decoded from the JWT), so they stay reactive across tenant switches.
export function usePermissions () {
  const auth = useAuthStore();
  const has = (permission) => auth.hasPermission(permission);
  const hasAny = (permissions) => auth.hasAnyPermission(permissions);
  return { has, hasAny };
}
