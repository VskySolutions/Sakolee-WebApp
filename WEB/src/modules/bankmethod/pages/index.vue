<template>
  <!-- 
    ============================================================
    Billing Methods Index Page Component
    ============================================================
    Manages billing method listings, filtering, server-side data grid operations,
    and side-drawers for viewing, creating, and editing billing methods.
  -->
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Billing Methods' }
      ]"
      title="Billing Methods"
      description="Manage your all billing methods here."
      :search="search"
      show-search
      search-placeholder="Search billing methods"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Billing Method"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <!-- Filter Drawer Component: Advanced filtering and deleted records controls -->
    <app-filter-drawer
      v-model="filterOpen"
      :chips="filterChips"
      @remove="removeFilter"
      @clear="clearFilters"
    >
      <app-column-filters
        v-model="filters"
        :columns="filterableColumns"
      />

      <q-toggle
        v-if="canManageDeleted"
        v-model="showDeleted"
        label="Show deleted?"
        dense
        class="q-mt-md"
      />
    </app-filter-drawer>

    <!-- Core Data Table Grid Component: Handles server-side pagination, sorting, row selection, and dataset display -->
    <app-data-table
      page-key="billing-methods"
      :row-key="(row) => row.billingMethodId || row.BillingMethodId || row.id || row.Id"
      title="Billing Methods"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Status Column Slot: Renders Active/Inactive badge indicators -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">
            {{ cell.value ? "Active" : "Inactive" }}
          </q-badge>
        </q-td>
      </template>

      <!-- Row Actions Slot: Renders individual record operations including view details, editing, and deletion -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>

          <q-btn
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removeBillingMethod(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- =========================================================
         Create / Edit Billing Method Form Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="formOpen"
      :title="editingId ? 'Edit Billing Method' : 'Create Billing Method'"
      :saving="saving"
      :save-label="editingId ? 'Save' : 'Create'"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <!-- Primary Entity Property: Billing Method Name -->
        <app-text-field
          v-model="form.name"
          label="Billing Method Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Billing method name is required',
            (v) =>
              !v ||
              v.trim().length <= 100 ||
              'Billing method name cannot exceed 100 characters'
          ]"
        />

        <!-- Status Toggle -->
        <q-toggle
          v-model="form.active"
          label="Active"
        />
      </q-form>
    </app-form-drawer>

    <!-- =========================================================
         View Billing Method Details Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="viewOpen"
      title="View Billing Method"
      :saving="viewLoading"
      :save-label="''"
      :hide-save="true"
      @cancel="closeView"
    >
      <!-- Loading Spinner Overlay during asynchronous fetch operations -->
      <div v-if="viewLoading" class="row flex-center q-pa-xl">
        <q-spinner color="primary" size="40px" />
      </div>

      <div v-else class="q-gutter-md">
        <!-- Field: Billing Method Name -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Billing Method Name
          </div>
          <div class="text-2e fs-4">
            {{ viewBillingMethod.name || '—' }}
          </div>
        </div>

        <!-- Field: Status -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :color="viewBillingMethod.active ? 'positive' : 'grey'">
            {{ viewBillingMethod.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <!-- Field: Created By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingMethod.createdBy || "—" }}
          </div>
        </div>

        <!-- Field: Created On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewBillingMethod.createdOnUtc) }}
          </div>
        </div>

        <!-- Field: Updated By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewBillingMethod.updatedBy || "—" }}
          </div>
        </div>

        <!-- Field: Updated On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewBillingMethod.updatedOnUtc) }}
          </div>
        </div>
      </div>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";

import {
  billingMethodApi,
  getApiErrorMessage,
  getApiErrorCode,
  ApiErrorCodes
} from "services/api";

import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppTextField from "components/common/AppTextField.vue";

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

/**
 * Standard date formatter matching global display expectations.
 * @param {String|Date} value - Raw date string or timestamp.
 */
const formatDate = (value) => {
  if (!value) {
    return "—";
  }
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

/*
 * ------------------------------------------------------------
 * Table columns definition (filterable: false set on non-target columns)
 * ------------------------------------------------------------
 */
const columns = [
  {
    name: "name",
    label: "Billing Method Name",
    field: (r) => r.name || r.Name || r.billingMethodName || r.BillingMethodName,
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "active",
    label: "Status",
    field: (r) => r.active ?? r.Active ?? r.is_active ?? true,
    align: "left",
    sortable: true,
    default: true,
    filterOptions: [
      { label: "Active", value: true },
      { label: "Inactive", value: false }
    ]
  },
  {
    name: "createdBy",
    label: "Created By",
    field: (r) => r.createdBy || r.CreatedBy || r.created_by || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false // Excluded from column filters
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || r.CreatedOn || r.created_on || null,
    align: "left",
    sortable: true,
    default: true,
    filterable: false, // Excluded from column filters
    format: (val) => formatDate(val)
  },
  {
    name: "updatedBy",
    label: "Updated By",
    field: (r) => r.updatedBy || r.UpdatedBy || r.updated_by || "—",
    align: "left",
    sortable: true,
    default: false,
    filterable: false // Excluded from column filters
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: (r) => r.updatedOnUtc || r.UpdatedOnUtc || r.updated_on_utc || r.updatedOn || r.UpdatedOn || r.updated_on || null,
    align: "left",
    sortable: true,
    default: false,
    filterable: false, // Excluded from column filters
    format: (val) => formatDate(val)
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

/*
 * ------------------------------------------------------------
 * List Table Data Management Composable
 * ------------------------------------------------------------
 */
const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "billing-methods",

  fetcher: ({ sortBy, descending }) =>
    billingMethodApi.list({
      search: search.value || undefined,
      sortBy: sortBy || 'createdOnUtc',
      descending: descending ?? true
    }).then((response) => {
      const items = response?.data?.items || response?.items || response?.data || [];
      return {
        data: items,
        total: items.length
      };
    }),

  onError: (err) => notify.error(getApiErrorMessage(err))
});

const filterOpen = ref(false);

const {
  filters,
  filterableColumns,
  filteredRows,
  filterChips,
  removeFilter,
  clearFilters
} = useColumnFilters(columns, rows, {
  server: false
});

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);

onMounted(() => {
  pagination.value.sortBy = 'createdOnUtc';
  pagination.value.descending = true;
  load();
});

/*
 * ------------------------------------------------------------
 * View Billing Method Drawer State & Handler Methods
 * ------------------------------------------------------------
 */
const viewOpen = ref(false);
const viewLoading = ref(false);

const viewBillingMethod = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const resetViewBillingMethod = () => {
  viewBillingMethod.id = null;
  viewBillingMethod.name = "";
  viewBillingMethod.active = true;
  viewBillingMethod.createdBy = "";
  viewBillingMethod.createdOnUtc = null;
  viewBillingMethod.updatedBy = "";
  viewBillingMethod.updatedOnUtc = null;
};

const openView = async (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }

  resetViewBillingMethod();
  viewOpen.value = true;
  viewLoading.value = true;

  try {
    const response = await billingMethodApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewBillingMethod.id = id;
      viewBillingMethod.name = item.name || item.Name || item.billingMethodName || item.BillingMethodName || "";
      viewBillingMethod.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewBillingMethod.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewBillingMethod.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewBillingMethod.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewBillingMethod.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
    }
  } catch (err) {
    viewOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

const closeView = () => {
  viewOpen.value = false;
  resetViewBillingMethod();
};

/*
 * ------------------------------------------------------------
 * Create / Edit Form Drawer State & Persistence Handlers
 * ------------------------------------------------------------
 */
const formOpen = ref(false);
const saving = ref(false);
const editingId = ref(null);
const formRef = ref(null);

const form = reactive({
  name: "",
  active: true
});

const resetForm = () => {
  editingId.value = null;
  form.name = "";
  form.active = true;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const openEdit = (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }

  editingId.value = id;
  form.name = row.name || row.Name || row.billingMethodName || row.BillingMethodName || "";
  form.active = row.active ?? row.Active ?? row.is_active ?? true;

  formOpen.value = true;
};

const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    if (editingId.value) {
      await billingMethodApi.update(editingId.value, payload);
      notify.success("Billing method updated successfully.");
    } else {
      const response = await billingMethodApi.create(payload);
      notify.success("Billing method created successfully.");
      
      const createdItem = response?.data?.data || response?.data || response;
      if (createdItem && typeof createdItem === 'object') {
        rows.value.unshift(createdItem);
      }
    }

    clearDraft?.();
    formOpen.value = false;
    resetForm();

    pagination.value.sortBy = 'createdOnUtc';
    pagination.value.descending = true;
    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A billing method with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

/*
 * ------------------------------------------------------------
 * Deletion Handling Workflow
 * ------------------------------------------------------------
 */
const removeBillingMethod = async (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) return;

  const methodLabel = row.name || row.Name || row.billingMethodName || row.BillingMethodName || 'this billing method';
  const ok = await confirm({
    title: "Delete billing method",
    message: `Are you sure you want to delete the billing method "${methodLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) {
    return;
  }

  try {
    await billingMethodApi.delete(id);
    notify.success("Billing method deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>