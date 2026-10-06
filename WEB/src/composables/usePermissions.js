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
  FamiliesRead: "families.read",
  FamiliesWrite: "families.write",
  FamiliesDelete: "families.delete",
  FamilyStatusesRead: "familyStatuses.read",
  FamilyStatusesWrite: "familyStatuses.write",
  FamilyStatusesDelete: "familyStatuses.delete",
  SessionsRead: "sessions.read",
  SessionsWrite: "sessions.write",
  SessionsDelete: "sessions.delete",
  StudentsRead: "students.read",
  StudentsWrite: "students.write",
  StudentsDelete: "students.delete",
  ClassesRead: "classes.read",
  ClassesWrite: "classes.write",
  ClassesDelete: "classes.delete",
  // Class Categories Permissions
  ClassCategoriesRead: "classCategories.read",
  ClassCategoriesWrite: "classCategories.write",
  ClassCategoriesDelete: "classCategories.delete",
  // Billing Cycles Permissions
  BillingCyclesRead: "billingCycles.read",
  BillingCyclesWrite: "billingCycles.write",
  BillingCyclesDelete: "billingCycles.delete",
  // T-Shirt Size permissions.
  TShirtSizesRead: "tShirtSizes.read",
  TShirtSizesWrite: "tShirtSizes.write",
  TShirtSizesDelete: "tShirtSizes.delete",
  // Hear About Us Permissions
  HearAboutUsRead: "hearAboutUs.read",
  HearAboutUsWrite: "hearAboutUs.write",
  HearAboutUsDelete: "hearAboutUs.delete",
  // E-Payment Schedule permissions
  EPaymentSchedulesRead: "ePaymentSchedules.read",
  EPaymentSchedulesWrite: "ePaymentSchedules.write",
  EPaymentSchedulesDelete: "ePaymentSchedules.delete",
  // Account Type permissions
  AccountTypesRead: "accountTypes.read",
  AccountTypesWrite: "accountTypes.write",
  AccountTypesDelete: "accountTypes.delete",
  // Locations Permissions
  LocationsRead: "locations.read",
  LocationsWrite: "locations.write",
  LocationsDelete: "locations.delete",

  // // Family Statuses Permissions
  // FamilyStatusesRead: "familyStatuses.read",
  // FamilyStatusesWrite: "familyStatuses.write",
  // FamilyStatusesDelete: "familyStatuses.delete",
  // Class Sessions Permissions
  ClassSessionsRead: "classSessions.read",
  ClassSessionsWrite: "classSessions.write",
  ClassSessionsDelete: "classSessions.delete",
  // Bank Methods Permissions
  BillingMethodsRead: "billingMethods.read",
  BillingMethodsWrite: "billingMethods.write",
  BillingMethodsDelete: "billingMethods.delete",

  // Family Relations Permissions
  FamilyRelationsRead: " familyRelations.read",
  FamilyRelationsWrite: "familyRelations.write",
  FamilyRelationsDelete: "familyRelations.delete",

  // Class Rooms Permissions
  ClassRoomsRead: "classRooms.read",
  ClassRoomsWrite: "classRooms.write",
  ClassRoomsDelete: "classRooms.delete",

  MembershipTypesRead: "membershipTypes.read",
  MembershipTypesWrite: "membershipTypes.write",
  MembershipTypesDelete: "membershipTypes.delete",

  // StudentGradeLevel
  StudentGradeLevelsRead: "studentGradeLevels.read",
  StudentGradeLevelsWrite: "studentGradeLevels.write",
  StudentGradeLevelsDelete: "studentGradeLevels.delete",

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
