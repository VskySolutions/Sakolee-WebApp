<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Tenants' }]"
      :search="search"
      show-search
      search-placeholder="Search name or identifier"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Tenant"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select
        v-model="filters.status"
        :options="statusFilterOptions"
        label="Status"
      />
      <q-toggle v-model="filters.includeArchived" label="Show archived" />
      <q-toggle
        v-if="canManageDeleted" v-model="showDeleted" label="Show deleted?" dense class="q-mt-md"
      />
    </app-filter-drawer>

    <app-data-table
      page-key="tenants"
      row-key="tenantId"
      title="All Tenants"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      selectable
      @request="onRequest"
      @refresh="load"
      @update:selected="selected = $event"
    >
      <template #bulk-actions="{ selected: sel }">
        <q-btn flat dense no-caps color="positive" label="Activate" @click="bulkSetStatus(sel, true)" />
        <q-btn flat dense no-caps color="negative" label="Deactivate" @click="bulkSetStatus(sel, false)" />
      </template>

      <template #body-cell-status="cell">
        <q-td :props="cell">
          <q-badge :color="statusColor(cell.value)">{{ cell.value }}</q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" :to="{ name: 'tenant_detail', params: { id: cell.row.tenantId } }">
            <q-tooltip>View / Manage</q-tooltip>
          </q-btn>
          <!-- One button per action, all of them on the row. -->
          <q-btn type="a" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn
            type="a" flat round dense
            :color="cell.row.status === 'Active' ? 'grey-8' : 'positive'"
            :icon="cell.row.status === 'Active' ? 'o_block' : 'o_check_circle'"
            @click="setStatus(cell.row, cell.row.status !== 'Active')"
          >
            <q-tooltip>{{ cell.row.status === "Active" ? "Deactivate" : "Activate" }}</q-tooltip>
          </q-btn>
          <q-btn type="a" flat round dense color="negative" icon="o_archive" @click="archive(cell.row)">
            <q-tooltip>Archive</q-tooltip>
          </q-btn>
          <q-btn
            type="a" flat round dense color="primary" icon="o_forward_to_inbox"
            @click="sendCredentials(cell.row)"
          >
            <q-tooltip>Send Credentials</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <deleted-records-panel
      v-if="canManageDeleted" :entity-type="EntityType.Tenant" :show="showDeleted" @restored="load"
    />

    <!-- Create / Edit drawer -->
    <app-form-drawer
      v-model="formOpen"
      :title="editing ? 'Edit Tenant' : 'Create Tenant'"
      :saving="saving"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <app-text-field
          v-model="form.name"
          label="Name"
          required
          class="q-mb-md"
          :rules="[(v) => !!v || 'Name is required']"
        />
        <app-text-field
          v-model="form.identifier"
          label="Identifier"
          required
          :disable="editing"
          hint="Lowercase letters, numbers and hyphens"
          :error="!!identifierError"
          :error-message="identifierError"
          :rules="editing ? [] : [
            (v) => !!v || 'Identifier is required',
            (v) => /^[a-z0-9-]+$/.test(v) || 'Use lowercase letters, numbers and hyphens only'
          ]"
        />
        <app-select
          v-model="form.timeZoneId"
          label="Time Zone"
          required
          class="q-mt-md q-mb-md"
          use-input
          :clearable="false"
          :options="allZones"
        />

        <!-- The tenant's own address — same field set on create and edit. -->
        <q-card flat bordered class="profile-card q-mb-md">
          <q-card-section class="text-subtitle1 text-weight-medium">Address</q-card-section>
          <q-separator />
          <q-card-section>
            <!-- Not `extended`: the landmark / building / floor / unit boxes are off this page. -->
            <app-address-fields v-model="form.address" />
          </q-card-section>
        </q-card>

        <!-- Only on create: the default Administrator this tenant is created with (a Person + protected
             User login, "Administrator" role — cannot be deactivated or lose its role assignment, so the
             tenant always keeps a working admin). -->
        <q-card v-if="!editing" flat bordered class="profile-card">
          <q-card-section class="text-subtitle1 text-weight-medium">User Information</q-card-section>
          <div class="text-caption text-grey-7 q-px-md q-pb-sm">
            The tenant's default Administrator — created with the "Administrator" role. This account
            cannot be deactivated or removed, so the tenant always keeps a working admin login.
          </div>
          <q-separator />
          <q-card-section>
            <app-text-field v-model="form.firstName" label="First Name *" class="q-mb-md" :rules="nameRules('First name', { required: true })" />
            <app-text-field v-model="form.lastName" label="Last Name *" class="q-mb-md" :rules="nameRules('Last name', { required: true })" />
            <app-text-field
              v-model="form.userEmail" type="email" label="User Email *" class="q-mb-md"
              :rules="userEmailRules" required
              :error="!!emailError" :error-message="emailError"
            />
            <app-phone-input v-model="form.phoneNumber" v-model:country="form.countryCode" label="Phone Number" />
          </q-card-section>
        </q-card>
      </q-form>
    </app-form-drawer>

    <temp-password-dialog v-model="tempPwOpen" :password="tempPassword" />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";
import { tenantApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes, EntityType } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useAuditColumns } from "composables/useAuditColumns";
import { useTenantScope } from "composables/useTenantScope";
import { nameRules } from "utils/personName";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppPhoneInput from "components/common/AppPhoneInput.vue";
import AppAddressFields from "components/common/AppAddressFields.vue";
import TempPasswordDialog from "components/temp_password_dialog.vue";

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
// The toolbar's "View as" menu is rendered from a list this composable caches for the whole session, so
// anything on this page that adds a tenant, renames one or retires one has to tell it.
const { refreshTenants } = useTenantScope();

const columns = [
  { name: "name", label: "Name", field: "name", align: "left", sortable: true, default: true },
  { name: "identifier", label: "Identifier", field: "identifier", align: "left", sortable: true, default: true },
  { name: "status", label: "Status", field: "status", align: "left", sortable: true, default: true },
  { name: "timeZoneId", label: "Time Zone", field: "timeZoneId", align: "left", sortable: true },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const filters = reactive({ status: null, includeArchived: false });
const { rows, loading, totalRecords, selected, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "tenants",
  fetcher: ({ page, limit, sortBy, descending }) =>
    tenantApi.list({
      page,
      limit,
      sortBy,
      descending,
      includeArchived: filters.includeArchived,
      status: filters.status || undefined,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Server-side filtering: reload (debounced, first page) whenever the search box or a filter changes.
const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const statusColor = (status) => ({ Active: "positive", Inactive: "grey", Archived: "blue-grey" }[status] || "grey");
const statusFilterOptions = ["Active", "Inactive", "Archived"].map((s) => ({ label: s, value: s }));

const filterChips = computed(() => {
  const chips = [];
  if (filters.status) chips.push({ key: "status", label: `Status: ${filters.status}` });
  if (filters.includeArchived) chips.push({ key: "includeArchived", label: "Including archived" });
  return chips;
});

const removeFilter = (key) => {
  if (key === "status") filters.status = null;
  if (key === "includeArchived") filters.includeArchived = false;
};
const clearFilters = () => {
  filters.status = null;
  filters.includeArchived = false;
};

// ---- Create / Edit ----
const formOpen = ref(false);
const editing = ref(false);
const saving = ref(false);
const identifierError = ref("");
const emailError = ref("");
const formRef = ref(null);
// The default Administrator's one-time temporary password, shown once right after a successful create.
const tempPwOpen = ref(false);
const tempPassword = ref("");
// One blank address shape, reused for the initial form and every reset.
const blankAddress = () => ({
  countryCode: null,
  countryName: null,
  stateCode: null,
  stateName: null,
  cityName: null,
  postalCode: "",
  addressLine1: "",
  addressLine2: "",
  landmark: "",
  buildingName: "",
  floorNumber: "",
  unitNumber: ""
});

const form = reactive({
  tenantId: null,
  name: "",
  identifier: "",
  timeZoneId: "UTC",
  // Only asked on create — the default Administrator (Person + protected User login).
  firstName: "",
  lastName: "",
  userEmail: "",
  phoneNumber: "",
  countryCode: null,
  // Only asked on create — the tenant's own address.
  address: blankAddress()
});

// The whole list goes to AppSelect, which narrows it as you type: it filters in a computed rather than
// through QSelect's filter/done-callback round trip, so there is nothing left here to filter.
const allZones = typeof Intl.supportedValuesOf === "function" ? Intl.supportedValuesOf("timeZone") : ["UTC"];

const resetForm = () => {
  form.tenantId = null;
  form.name = "";
  form.identifier = "";
  form.timeZoneId = "UTC";
  form.firstName = "";
  form.lastName = "";
  form.userEmail = "";
  form.phoneNumber = "";
  form.countryCode = null;
  form.address = blankAddress();
  identifierError.value = "";
  emailError.value = "";
  editing.value = false;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

// The address is a mapped AddressResponse from the tenant's own record, so it maps straight onto the
// blank shape's keys.
const mapAddress = (a) => (a ? { ...blankAddress(), ...a } : blankAddress());

const openEdit = async (row) => {
  resetForm();
  editing.value = true;
  form.tenantId = row.tenantId;
  form.name = row.name;
  form.identifier = row.identifier;
  form.timeZoneId = row.timeZoneId || "UTC";
  formOpen.value = true;
  // The list row carries no address — read the full tenant for it.
  try {
    const detail = await tenantApi.get(row.tenantId);
    form.address = mapAddress(detail?.address);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

// The Address panel's fields are all optional; an all-blank card should not create an empty Address row.
const hasAddressValue = (address) => Object.values(address).some((v) => !!v);

const submitForm = async ({ clearDraft } = {}) => {
  identifierError.value = "";
  emailError.value = "";
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    if (editing.value) {
      await tenantApi.update(form.tenantId, {
        name: form.name,
        timeZoneId: form.timeZoneId,
        address: hasAddressValue(form.address) ? form.address : undefined
      });
      notify.success("Tenant updated.");
      clearDraft?.();
      formOpen.value = false;
      resetForm();
    } else {
      const result = await tenantApi.create({
        name: form.name,
        identifier: form.identifier,
        timeZoneId: form.timeZoneId,
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.userEmail,
        phoneNumber: form.phoneNumber || undefined,
        countryCode: form.countryCode || undefined,
        address: hasAddressValue(form.address) ? form.address : undefined
      });
      clearDraft?.();
      formOpen.value = false;
      resetForm();
      notify.success("Tenant created.");
      // The Administrator's one-time temporary password — shown once, same as Create User.
      tempPassword.value = result?.temporaryPassword || "";
      tempPwOpen.value = true;
    }
    load();
    refreshTenants();
  } catch (err) {
    const code = getApiErrorCode(err);
    if (code === ApiErrorCodes.DuplicateIdentifier) {
      identifierError.value = "This identifier is already in use.";
    } else if (code === ApiErrorCodes.DuplicateEmail) {
      emailError.value = "This email is already in use.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

// ---- Status / Archive ----
const setStatus = async (row, isActive) => {
  const ok = await confirm({
    title: isActive ? "Activate tenant" : "Deactivate tenant",
    message: `${isActive ? "Activate" : "Deactivate"} "${row.name}"?`,
    type: isActive ? "primary" : "danger"
  });
  if (!ok) return;
  try {
    await tenantApi.setStatus(row.tenantId, isActive);
    notify.success("Status updated.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const bulkSetStatus = async (sel, isActive) => {
  if (!sel.length) return;
  const ok = await confirm({
    title: isActive ? "Activate tenants" : "Deactivate tenants",
    message: `${isActive ? "Activate" : "Deactivate"} ${sel.length} tenant(s)?`,
    type: isActive ? "primary" : "danger"
  });
  if (!ok) return;
  try {
    await Promise.all(sel.map((r) => tenantApi.setStatus(r.tenantId, isActive)));
    notify.success("Tenants updated.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const archive = async (row) => {
  const ok = await confirm({
    title: "Archive tenant",
    message: `Archive "${row.name}"? This retires the tenant.`,
    confirmLabel: "Archive",
    type: "danger"
  });
  if (!ok) return;
  try {
    await tenantApi.archive(row.tenantId);
    notify.success("Tenant archived.");
    load();
    // An archived tenant drops out of the list the scope picker offers, so it must not stay on the menu.
    refreshTenants();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const sendCredentials = async (row) => {
  const ok = await confirm({
    title: "Send credentials",
    // Usually just re-sends the Administrator's existing login credentials unchanged. If none are on
    // file to resend (an older account, or one who already signed in and set their own password), a
    // fresh temporary password is minted instead — same as Reset Password.
    message: `Email "${row.name}"'s Administrator their login credentials (username and temporary password)?`,
    confirmLabel: "Send",
    type: "primary"
  });
  if (!ok) return;
  try {
    const result = await tenantApi.sendCredentials(row.tenantId);
    const sentText = result?.emailSent
      ? "Credentials emailed to the Administrator."
      : "No active email sender for this tenant — share the credentials below manually.";
    notify.success(result?.passwordWasReset
      ? `${sentText} A new temporary password was generated (none was on file to resend).`
      : sentText);
    // The plaintext temporary password is shown here too, same as Create/Reset.
    tempPassword.value = result?.temporaryPassword || "";
    tempPwOpen.value = true;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const userEmailRules = [
  (v) => !!v || "Email is required",
  (v) => /.+@.+\..+/.test(v) || "Enter a valid email"
];
</script>
