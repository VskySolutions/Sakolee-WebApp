<template>
  <q-page padding>
    <app-list-header
      v-model:search="search"
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'Roles' }]"
      title="Roles"
      description="Manage roles and their permissions."
      show-search
      search-placeholder="Search roles"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Role"
      show-back
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-column-filters v-model="filters" :columns="filterableColumns" />
      <q-toggle
        v-if="canManageDeleted" v-model="showDeleted" label="Show deleted?" dense class="q-mt-md"
      />
    </app-filter-drawer>

    <app-data-table
      page-key="roles"
      row-key="id"
      title="Roles"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-isSystem="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'blue-grey' : 'primary'">{{ cell.value ? "System" : "Custom" }}</q-badge>
        </q-td>
      </template>

      <!-- Actions: View, Edit & Delete -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <!-- View Button -->
          <q-btn
            flat round dense color="primary" icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View Details</q-tooltip>
          </q-btn>

          <!-- Edit Button (Only if canManage is true) -->
          <q-btn
            v-if="cell.row.canManage"
            flat round dense color="primary" icon="o_edit"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit Role</q-tooltip>
          </q-btn>

          <!-- Delete Button -->
          <q-btn
            v-if="cell.row.canManage && !cell.row.isSystem && !isFixedNameRole(cell.row)"
            type="a" flat round dense color="negative"
            icon="o_delete" @click="removeRole(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <deleted-records-panel
      v-if="canManageDeleted" :entity-type="EntityType.Role" :show="showDeleted" @restored="load"
    />

    <!-- Create / Edit Role Dialog -->
    <app-form-dialog
      v-model="formOpen"
      :title="isEditing ? 'Edit Role' : 'Create Role'"
      :saving="saving"
      :save-label="isEditing ? 'Save Changes' : 'Create Role'"
      size="md"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <div v-if="loadingForm" class="row flex-center q-pa-xl">
        <q-spinner color="primary" size="40px" />
      </div>

      <q-form v-else ref="formRef" greedy>
        <app-text-field
          v-model="form.name" label="Name" required class="q-mb-md"
          :rules="[(v) => !!v || 'Name is required']"
        />
        <app-text-field v-model="form.displayName" label="Display Name" class="q-mb-md" />
        <app-rich-text-field v-model="form.description" label="Description" class="q-mb-md" />
        <app-select
          v-model="form.permissions" :options="permissionOptions" label="Permissions" multiple
          :loading="loadingPermissions"
          :info="isSuperAdmin ? '' : 'The list stops at what your own tenant can hand out.'"
        />
      </q-form>
    </app-form-dialog>

    <!-- Role View Dialog (Inside q-page root) -->
    <role-view-dialog
      v-model="viewOpen"
      :role-id="selectedRoleId"
    />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";
import { roleApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes, EntityType } from "services/api";
import { useAuthStore } from "stores/auth";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useAuditColumns } from "composables/useAuditColumns";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import { stripHtml } from "utils/richText";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppRichTextField from "components/common/AppRichTextField.vue";
import RoleViewDialog from "modules/role/pages/detail.vue"; 

const auditColumns = useAuditColumns();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const authStore = useAuthStore();
const router = useRouter();

const isSuperAdmin = computed(() => authStore.roles.includes("SuperAdmin"));

// View Dialog State
const viewOpen = ref(false);
const selectedRoleId = ref(null);

const openView = (row) => {
  selectedRoleId.value = row.id;
  viewOpen.value = true;
};

const columns = [
  { name: "name", label: "Name", field: "name", align: "left", sortable: true, default: true },
  { name: "displayName", label: "Display Name", field: (r) => r.displayName || "—", align: "left", sortable: true, default: true },
  { name: "description", label: "Description", field: (r) => stripHtml(r.description), align: "left", default: true,filterable: false },
  {
    name: "isSystem",
    label: "Type",
    field: "isSystem",
    align: "left",
    sortable: true,
    default: true,
    filterOptions: [{ label: "System", value: true }, { label: "Custom", value: false }]
  },
  {
    name: "scope",
    label: "Scope",
    field: (r) => r.tenantName || "Platform",
    align: "left",
    sortable: true,
    default: true
  },
  { name: "permissionCount", label: "Permissions", field: "permissionCount", align: "left", sortable: true, default: true, filterable: false },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

// Table state and data fetching
const { rows, loading, search, pagination, load, onRequest } = useListTable({
  pageKey: "roles",
  fetcher: ({ sortBy, descending }) =>
    roleApi.list({ search: search.value || undefined, sortBy, descending })
      .then((r) => ({ data: r || [], total: (r || []).length })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Filters
const filterOpen = ref(false);
const { filters, filterableColumns, filteredRows, filterChips, removeFilter, clearFilters } = useColumnFilters(columns, rows, { server: false });

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch(search, reload);

// ---- Permission catalogue ----
const permissionOptions = ref([]);
const loadingPermissions = ref(false);
const prettyPermission = (key) => key.replace(/_/g, " ").replace(/\./g, " · ");

const loadPermissions = async () => {
  if (permissionOptions.value.length) return;
  loadingPermissions.value = true;
  try {
    const perms = await roleApi.permissions();
    permissionOptions.value = (perms || []).map((p) => ({ label: prettyPermission(p), value: p }));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loadingPermissions.value = false;
  }
};

const FIXED_NAME_ROLES = ["administrator", "parent"];
const isFixedNameRole = (row) => FIXED_NAME_ROLES.includes((row.name || "").trim().toLowerCase());

// ---- Create / Edit Dialog State ----
const formOpen = ref(false);
const saving = ref(false);
const loadingForm = ref(false);
const formRef = ref(null);
const editingId = ref(null);
const isEditing = computed(() => !!editingId.value);

const form = reactive({ name: "", displayName: "", description: "", permissions: [] });

const resetForm = () => {
  editingId.value = null;
  form.name = "";
  form.displayName = "";
  form.description = "";
  form.permissions = [];
  formOpen.value, (formOpen.value = false);
};

const openCreate = async () => {
  resetForm();
  await loadPermissions();
  formOpen.value = true;
};

const openEdit = async (row) => {
  editingId.value = row.id;
  await loadPermissions();
  formOpen.value = true;
  loadingForm.value = true;
  try {
    const detail = await roleApi.get(row.id);
    form.name = detail.name || "";
    form.displayName = detail.displayName || "";
    form.description = detail.description || "";
    form.permissions = detail.permissions || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loadingForm.value = false;
  }
};

// Watchers to handle prop changes and open/close state
const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) return;
  saving.value = true;
  try {
    const payload = {
      name: form.name,
      displayName: form.displayName || undefined,
      description: form.description,
      permissions: form.permissions
    };

    if (isEditing.value) {
      await roleApi.update(editingId.value, payload);
      notify.success("Role updated successfully.");
      load();
    } else {
      const created = await roleApi.create(payload);
      notify.success("Role created.");
      if (created?.id) {
        router.push({ name: "role_detail", params: { id: created.id } });
      } else {
        load();
      }
    }
    clearDraft?.();
    formOpen.value = false;
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A role with that name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

// Function to remove a role with confirmation
const removeRole = async (row) => {
  const ok = await confirm({
    title: "Delete role",
    message: `Delete the "${row.name}" role? Users keeping this role will lose its permissions.`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await roleApi.remove(row.id);
    notify.success("Role deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>