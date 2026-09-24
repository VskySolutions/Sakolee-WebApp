<template>
  <!-- 
    ============================================================
    Family Status Index Page Component
    ============================================================
    Manages family status listings, filtering, server-side data grid operations,
    and side-drawers for viewing, creating, and editing family statuses.
  -->
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Family Status' }
      ]"
      title="Family Status"
      description="Manage your all family statuses here."
      :search="search"
      show-search
      search-placeholder="Search family status"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Family Status"
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
      page-key="family-statuses"
      :row-key="(row) => row.familyStatusId || row.FamilyStatusId || row.id || row.Id"
      title="Family Status"
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
            @click="removeFamilyStatus(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- =========================================================
         Create / Edit Family Status Form Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="formOpen"
      :title="editingId ? 'Edit Family Status' : 'Create Family Status'"
      :saving="saving"
      :save-label="editingId ? 'Save' : 'Create'"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <!-- Primary Entity Property: Family Status Name -->
        <app-text-field
          v-model="form.familyStatusName"
          label="Family Status Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Family status name is required',
            (v) =>
              !v ||
              v.trim().length <= 100 ||
              'Family status name cannot exceed 100 characters'
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
         View Family Status Details Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="viewOpen"
      title="View Family Status"
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
        <!-- Field: Family Status Name -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Family Status Name
          </div>
          <div class="text-2e fs-4">
            {{ viewFamilyStatus.familyStatusName || '—' }}
          </div>
        </div>

        <!-- Field: Status -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :color="viewFamilyStatus.active ? 'positive' : 'grey'">
            {{ viewFamilyStatus.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <!-- Field: Created By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewFamilyStatus.createdBy || "—" }}
          </div>
        </div>

        <!-- Field: Created On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewFamilyStatus.createdOnUtc) }}
          </div>
        </div>

        <!-- Field: Updated By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewFamilyStatus.updatedBy || "—" }}
          </div>
        </div>

        <!-- Field: Updated On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewFamilyStatus.updatedOnUtc) }}
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
  familyStatusApi,
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
 * Table columns definition (filterable: false set on audit columns)
 * ------------------------------------------------------------
 */
const columns = [
  {
    name: "familyStatusName",
    label: "Family Status Name",
    field: (r) => r.familyStatusName || r.FamilyStatusName || r.name || r.Name,
    align: "left",
    sortable: true,
    default: true
  },
  // {
  //   name: "active",
  //   label: "Status",
  //   field: (r) => r.active ?? r.Active ?? r.is_active ?? true,
  //   align: "left",
  //   sortable: true,
  //   default: true,
  //   filterOptions: [
  //     { label: "Active", value: true },
  //     { label: "Inactive", value: false }
  //   ]
  // },
 
  {
    name: "createdOnUtc",
    label: "Created On",
    field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || r.CreatedOn || r.created_on || null,
    align: "left",
    sortable: true,
    default: true,
    filterable: false,
    format: (val) => formatDate(val)
  },

   {
    name: "createdBy",
    label: "Created By",
    field: (r) => r.createdBy || r.CreatedBy || r.created_by || "—",
    align: "left",
    sortable: true,
    default: true,
    filterable: false
  },
  
  {
    name: "updatedBy",
    label: "Updated By",
    field: (r) => r.updatedBy || r.UpdatedBy || r.updated_by || "—",
    align: "left",
    sortable: true,
    default: false,
    filterable: false
  },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: (r) => r.updatedOnUtc || r.UpdatedOnUtc || r.updated_on_utc || r.updatedOn || r.UpdatedOn || r.updated_on || null,
    align: "left",
    sortable: true,
    default: false,
    filterable: false,
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
 * List Table Data Management Composable with Robust Sorting
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
  pageKey: "family-statuses",
  defaultSortBy: "createdOnUtc",
  defaultDescending: true,

  fetcher: (params) => {
    const sortBy = params?.sortBy || pagination.value.sortBy || "createdOnUtc";
    const descending = params?.descending ?? pagination.value.descending ?? true;

    return familyStatusApi.list({
      search: search.value || undefined,
      sortBy,
      descending
    }).then((response) => {
      let items = response?.data?.items || response?.items || response?.data || [];

      // 100% Guaranteed Strict Client-Side Sorting to show newest/newly-created on top
      items.sort((a, b) => {
        const fieldA = pagination.value.sortBy || sortBy;
        const isDesc = pagination.value.descending ?? descending;

        if (fieldA === 'createdOnUtc' || fieldA === 'updatedOnUtc' || fieldA === 'CreatedOnUtc') {
          const valA = new Date(a.createdOnUtc || a.CreatedOnUtc || a.created_on_utc || a.createdOn || 0).getTime();
          const valB = new Date(b.createdOnUtc || b.CreatedOnUtc || b.created_on_utc || b.createdOn || 0).getTime();
          
          if (valA !== valB) {
            return isDesc ? valB - valA : valA - valB;
          }
        }

        // Fallback sort by ID (newer IDs are usually larger numbers)
        const idA = a.familyStatusId || a.FamilyStatusId || a.id || a.Id || 0;
        const idB = b.familyStatusId || b.FamilyStatusId || b.id || b.Id || 0;
        
        if (typeof idA === 'number' && typeof idB === 'number' && idA !== idB) {
          return isDesc ? idB - idA : idA - idB;
        }

        // Secondary fallback by name string
        const nameA = String(a.familyStatusName || a.FamilyStatusName || a.name || '').toLowerCase();
        const nameB = String(b.familyStatusName || b.FamilyStatusName || b.name || '').toLowerCase();
        return isDesc ? nameB.localeCompare(nameA) : nameA.localeCompare(nameB);
      });

      return {
        data: items,
        total: response?.data?.totalCount || items.length
      };
    });
  },

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
 * View Family Status Drawer State & Handler Methods
 * ------------------------------------------------------------
 */
const viewOpen = ref(false);
const viewLoading = ref(false);

const viewFamilyStatus = reactive({
  id: null,
  familyStatusName: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const resetViewFamilyStatus = () => {
  viewFamilyStatus.id = null;
  viewFamilyStatus.familyStatusName = "";
  viewFamilyStatus.active = true;
  viewFamilyStatus.createdBy = "";
  viewFamilyStatus.createdOnUtc = null;
  viewFamilyStatus.updatedBy = "";
  viewFamilyStatus.updatedOnUtc = null;
};

const openView = async (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }

  resetViewFamilyStatus();
  viewOpen.value = true;
  viewLoading.value = true;

  try {
    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewFamilyStatus.id = id;
      viewFamilyStatus.familyStatusName = item.familyStatusName || item.FamilyStatusName || item.name || item.Name || "";
      viewFamilyStatus.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewFamilyStatus.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewFamilyStatus.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewFamilyStatus.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewFamilyStatus.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
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
  resetViewFamilyStatus();
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
  familyStatusName: "",
  active: true
});

const resetForm = () => {
  editingId.value = null;
  form.familyStatusName = "";
  form.active = true;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const openEdit = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }

  editingId.value = id;
  form.familyStatusName = row.familyStatusName || row.FamilyStatusName || row.name || row.Name || "";
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
      name: form.familyStatusName.trim(),
      familyStatusName: form.familyStatusName.trim(),
      active: form.active
    };

    if (editingId.value) {
      await familyStatusApi.update(editingId.value, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
    }

    clearDraft?.();
    formOpen.value = false;
    resetForm();

    // Reset pagination to first page and reload to fetch properly sorted fresh records from backend/frontend
    pagination.value.page = 1;
    pagination.value.sortBy = 'createdOnUtc';
    pagination.value.descending = true;
    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A family status with this name already exists.");
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
const removeFamilyStatus = async (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id || row.Id;
  if (!id) return;

  const familyStatusLabel = row.familyStatusName || row.FamilyStatusName || row.name || row.Name || 'this family status';
  const ok = await confirm({
    title: "Delete family status",
    message: `Are you sure you want to delete the family status "${familyStatusLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) {
    return;
  }

  try {
    await familyStatusApi.delete(id);
    notify.success("Family status deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>