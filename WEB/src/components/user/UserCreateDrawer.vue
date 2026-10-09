<template>
  <div>
    <!-- Create user (creates its own Person master record from the name/email below) -->
    <app-form-dialog v-model="open" title="Create Staff" size="md" :saving="saving" @submit="submitForm" @cancel="resetForm">
      <q-form ref="formRef" greedy>
        <!-- There is no existing Person to promote — one is created inline from these fields. -->
        <div class="row q-col-gutter-md q-mb-md">
          <app-text-field
            v-model="form.firstName" label="First Name *" class="col-12 col-sm-6"
            :rules="nameRules('First name', { required: true })"
          />
          <app-text-field
            v-model="form.lastName" label="Last Name *" class="col-12 col-sm-6"
            :rules="nameRules('Last name', { required: true })"
          />
        </div>
        <app-text-field
          v-model="form.email" type="email" label="Email *" required class="q-mb-md"
          hint="The user signs in with this email."
          :error="!!emailError" :error-message="emailError"
          :rules="[(v) => !!v || 'Email is required', (v) => /.+@.+\..+/.test(v) || 'Enter a valid email']"
        />
        <!-- Hidden when the caller fixes the role (the Staff page always creates "Instructor" accounts). -->
        <app-select
          v-if="!defaultRole" v-model="form.roleIds" :options="roleOptions" label="Roles *" multiple class="q-mb-md"
          :loading="loadingRoles" hint="Grouped by category. Assign one or more roles."
          info="The roles assignable in the first selected tenant, grouped System / Operational / Custom. Super Admin is only listed for a Super Admin."
        />

        <!-- Department + groups, the same placements the user's detail page manages. -->
        <template v-if="showDepartmentAndGroups && inActiveTenant">
          <app-select
            v-model="form.department" :options="departmentOptions" :loading="loadingDepartments"
            label="Department" class="q-mb-md"
            info="From the Department option list (Tenant Settings → Option Sets). A department has one head."
            @update:model-value="onDepartmentChange"
          />
          <q-toggle v-model="form.isDepartmentHead" :disable="!form.department" label="Department head" />
          <div class="text-caption text-grey-7 q-mb-md">{{ headHint }}</div>

          <app-select
            v-if="canManageGroups" v-model="form.groupIds" :options="groupOptions" label="Groups" multiple
            class="q-mb-md" :loading="loadingGroups"
            hint="Tenant user groups (segmentation, independent of roles)."
            info="Groups in your active tenant, maintained in Administration → User Groups. Membership is what scopes group-based pickers."
          />
        </template>

        <!-- Send invitation toggle hidden for now. -->
        <q-toggle
          v-model="form.sendInvitation" color="primary"
          label="Send invitation email with the temporary password"
        />
        <div class="text-caption text-grey-7 q-mb-md">
          Emails the user their login link and temporary password via the tenant's active SMTP account.
        </div>
       
      </q-form>
    </app-form-dialog>

    <temp-password-dialog v-model="tempPwOpen" :password="tempPassword" />
  </div>
</template>

<script setup>
// The Create User drawer: creates its own Person master record from a name/email, gives it roles, and
// optionally places it in a department and some groups.
import { ref, reactive, computed, watch } from "vue";
import { userApi, userGroupApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { usePermissions, Permissions } from "composables/usePermissions";
import { useTenantOptions } from "composables/useTenantOptions";
import { useTenantScope } from "composables/useTenantScope";
import { useRoleOptions } from "composables/useRoleOptions";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { nameRules } from "utils/personName";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import TempPasswordDialog from "components/temp_password_dialog.vue";

const props = defineProps({
  // The tenant the account is created in. Null means the caller's active (selected) tenant.
  tenantId: { type: String, default: null },
  // A role name to assign instead of asking: the Roles picker is hidden and this role, looked up by name
  // in the target tenant, is the account's only role.
  defaultRole: { type: String, default: null }
});
const emit = defineEmits(["created"]);

const open = defineModel({ type: Boolean, default: false });

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
// No tenant picker: the account always goes into the given tenant, else the one selected in the header
// (Super-Admin scope, falling back to the caller's active tenant).
const { activeTenantId } = useTenantOptions();
const { selectedTenantId } = useTenantScope();
const canManageGroups = computed(() => has(Permissions.UsersGroupManagement));
// Department and Groups fields are hidden for now; flip to true to bring them back.
const showDepartmentAndGroups = false;

const saving = ref(false);
const emailError = ref("");
const formRef = ref(null);
const form = reactive({
  firstName: "",
  lastName: "",
  email: "",
  roleIds: [],
  sendInvitation: false,
  // Tenant-scoped placements, applied through their own endpoints once the account exists.
  department: null,
  isDepartmentHead: false,
  groupIds: []
});
// Grouped, category-labelled multi-role options (SuperAdmin excluded for non-Super-Admin callers).
const { roleOptions, loading: loadingRoles, loadForTenant } = useRoleOptions();

// The tenant the user is being created in: fixed by the caller, else the header-selected tenant. Roles
// are populated from it and the login's UserTenantRole is created in it.
const targetTenantIds = computed(() => {
  const tenantId = props.tenantId || selectedTenantId.value;
  return tenantId ? [tenantId] : [];
});
const primaryTenantId = computed(() => targetTenantIds.value[0] || null);

// ---- Department & groups (as on the user's detail page) ----
// Both live in the caller's ACTIVE tenant: the pickers are loaded from it and the endpoints require the
// user to hold an assignment there.
const inActiveTenant = computed(() => !!activeTenantId.value && primaryTenantId.value === activeTenantId.value);

const departmentOptions = ref([]);
const departmentHeads = ref([]);
const loadingDepartments = ref(false);
const groupOptions = ref([]);
const loadingGroups = ref(false);

const departmentLabel = (code) => departmentOptions.value.find((o) => o.value === code)?.label || code;
const currentHead = computed(() => departmentHeads.value.find((h) => h.department === form.department) || null);

const headHint = computed(() => {
  if (!form.department) return "Pick a department to set a head.";
  if (!currentHead.value) return `${departmentLabel(form.department)} has no head yet.`;
  return `${currentHead.value.fullName} currently heads ${departmentLabel(form.department)}.`;
});

// Headship is meaningless without a department, and is never carried across a change of one.
const onDepartmentChange = (value) => {
  form.department = value;
  form.isDepartmentHead = false;
};

const loadDepartments = async () => {
  loadingDepartments.value = true;
  try {
    const result = await userApi.departments();
    departmentOptions.value = result?.departments || [];
    departmentHeads.value = result?.heads || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loadingDepartments.value = false;
  }
};

const loadGroups = async () => {
  loadingGroups.value = true;
  try {
    const groups = (await userGroupApi.list()) || [];
    groupOptions.value = groups.map((g) => ({ label: g.name, value: g.id }));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loadingGroups.value = false;
  }
};

// Applied once the account exists, through the same endpoints the detail page uses. Reported but never
// fatal: the user has already been created, and the temporary password below is shown only once.
const applyPlacements = async (newUserId) => {
  if (!newUserId || !inActiveTenant.value) return "";
  const notes = [];
  try {
    if (canManageGroups.value && form.groupIds.length) {
      await userApi.setGroups(newUserId, form.groupIds);
    }
    if (form.department) {
      const result = await userApi.setDepartment(newUserId, {
        department: form.department,
        isHead: form.isDepartmentHead
      });
      if (result?.demotedHeadName) notes.push(`${result.demotedHeadName} is no longer the department head.`);
    }
  } catch (err) {
    notify.warning(`User created, but the department/groups could not be applied: ${getApiErrorMessage(err)}`);
  }
  return notes.join(" ");
};

// Role options come from the PRIMARY tenant's assignable roles (system + custom), grouped by category.
const loadRoles = async () => {
  form.roleIds = [];
  try {
    await loadForTenant(primaryTenantId.value);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const resetForm = () => {
  form.firstName = "";
  form.lastName = "";
  form.email = "";
  form.roleIds = [];
  form.sendInvitation = false;
  form.department = null;
  form.isDepartmentHead = false;
  form.groupIds = [];
  emailError.value = "";
};

// Everything the drawer offers depends on which tenant the account is going into, so it is all loaded when
// the drawer opens rather than once on mount: the same drawer is opened again for a different tenant.
watch(open, async (isOpen) => {
  if (!isOpen) return;
  resetForm();
  await Promise.all([
    loadRoles(),
    // Both pickers come from the caller's active tenant, so they are pointless without one (a Super Admin
    // who has not switched in) — that is also exactly when the section stays hidden.
    activeTenantId.value ? loadDepartments() : Promise.resolve(),
    activeTenantId.value && canManageGroups.value ? loadGroups() : Promise.resolve()
  ]);
});

const tempPwOpen = ref(false);
const tempPassword = ref("");

const submitForm = async ({ clearDraft } = {}) => {
  emailError.value = "";
  if (!(await formRef.value?.validate())) return;
  let roleIds = form.roleIds;
  if (props.defaultRole) {
    const wanted = props.defaultRole.toLowerCase();
    const match = roleOptions.value.find((o) => !o.header && o.label?.toLowerCase() === wanted);
    if (!match) {
      notify.error(`No "${props.defaultRole}" role exists in this tenant. Create it under Access Management → Roles first.`);
      return;
    }
    roleIds = [match.value];
  }
  if (!roleIds.length) {
    notify.error("Select at least one role.");
    return;
  }
  const tenantIds = targetTenantIds.value;
  if (!tenantIds.length) {
    notify.error("Select a tenant first (use the tenant switcher), then create the staff member.");
    return;
  }
  // A department has one head, so taking it demotes the incumbent — name them before anything is created.
  if (inActiveTenant.value && form.isDepartmentHead && currentHead.value) {
    const ok = await confirm({
      title: "Change department head",
      message: `${currentHead.value.fullName} currently heads ${departmentLabel(form.department)}. Make the ` +
        `new user the head instead? ${currentHead.value.fullName} will no longer head the department.`,
      confirmLabel: "Make head"
    });
    if (!ok) return;
  }
  saving.value = true;
  try {
    const payload = {
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      roleIds,
      tenantIds,
      sendInvitation: form.sendInvitation
    };
    const recipientEmail = form.email;
    const wantedInvite = form.sendInvitation;
    const result = await userApi.create(payload);
    // Before resetForm() clears the picks it reads.
    const placementNote = await applyPlacements(result?.userId);
    clearDraft?.();
    open.value = false;
    resetForm();
    tempPassword.value = result?.temporaryPassword || "";
    tempPwOpen.value = true;
    if (placementNote) notify.info(placementNote);
    if (wantedInvite) {
      if (result?.invitationEmailSent) {
        notify.success(`Invitation email sent to ${recipientEmail}.`);
      } else {
        notify.warning("User created, but the invitation email could not be sent (no active SMTP account). Share the temporary password manually.");
      }
    }
    emit("created", result);
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      emailError.value = "This email is already in use.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>
