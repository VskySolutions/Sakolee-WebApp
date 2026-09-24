<template>
  <!-- 
    ============================================================
    Sessions Index Page Component
    ============================================================
    Manages session listings, filtering, server-side data grid operations,
    and side-drawers for viewing, creating, and editing sessions.
  -->
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Sessions' }
      ]"
      title="Sessions"
      description="Manage your all sessions here."
      :search="search"
      show-search
      search-placeholder="Search sessions"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Session"
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
      page-key="sessions"
      :row-key="(row) => row.sessionId || row.SessionId || row.id || row.Id"
      title="Sessions"
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
            @click="removeSession(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- =========================================================
         Create / Edit Session Form Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="formOpen"
      :title="editingId ? 'Edit Session' : 'Create Session'"
      :saving="saving"
      :save-label="editingId ? 'Save' : 'Create'"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <!-- Primary Entity Property: Session Name -->
        <app-text-field
          v-model="form.sessionName"
          label="Session Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Session name is required',
            (v) =>
              !v ||
              v.trim().length <= 100 ||
              'Session name cannot exceed 100 characters'
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
         View Session Details Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="viewOpen"
      title="View Session"
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
        <!-- Field: Session Name -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Session Name
          </div>
          <div class="text-2e fs-4">
            {{ viewSession.sessionName || '—' }}
          </div>
        </div>

        <!-- Field: Status -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :color="viewSession.active ? 'positive' : 'grey'">
            {{ viewSession.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <!-- Field: Created By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewSession.createdBy || "—" }}
          </div>
        </div>

        <!-- Field: Created On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewSession.createdOnUtc) }}
          </div>
        </div>

        <!-- Field: Updated By -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewSession.updatedBy || "—" }}
          </div>
        </div>

        <!-- Field: Updated On -->
        <div>
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewSession.updatedOnUtc) }}
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
  classSessionApi,
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
    name: "sessionName",
    label: "Session Name",
    field: (r) => r.sessionName || r.SessionName || r.name || r.Name,
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
  pageKey: "sessions",

  fetcher: ({ sortBy, descending }) =>
    classSessionApi.list({
      search: search.value || undefined,
      sortBy,
      descending
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
  load();
});

/*
 * ------------------------------------------------------------
 * View Session Drawer State & Handler Methods
 * ------------------------------------------------------------
 */
const viewOpen = ref(false);
const viewLoading = ref(false);

const viewSession = reactive({
  id: null,
  sessionName: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const resetViewSession = () => {
  viewSession.id = null;
  viewSession.sessionName = "";
  viewSession.active = true;
  viewSession.createdBy = "";
  viewSession.createdOnUtc = null;
  viewSession.updatedBy = "";
  viewSession.updatedOnUtc = null;
};

const openView = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }

  resetViewSession();
  viewOpen.value = true;
  viewLoading.value = true;

  try {
    const response = await classSessionApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewSession.id = id;
      viewSession.sessionName = item.sessionName || item.SessionName || item.name || item.Name || "";
      viewSession.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewSession.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewSession.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewSession.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewSession.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
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
  resetViewSession();
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
  sessionName: "",
  active: true
});

const resetForm = () => {
  editingId.value = null;
  form.sessionName = "";
  form.active = true;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const openEdit = (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }

  editingId.value = id;
  form.sessionName = row.sessionName || row.SessionName || row.name || row.Name || "";
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
      name: form.sessionName.trim(),
      sessionName: form.sessionName.trim(),
      active: form.active
    };

    if (editingId.value) {
      await classSessionApi.update(editingId.value, payload);
      notify.success("Session updated successfully.");
    } else {
      await classSessionApi.create(payload);
      notify.success("Session created successfully.");
    }

    clearDraft?.();
    formOpen.value = false;
    resetForm();

    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A session with this name already exists.");
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
const removeSession = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) return;

  const sessionLabel = row.sessionName || row.SessionName || row.name || row.Name || 'this session';
  const ok = await confirm({
    title: "Delete session",
    message: `Are you sure you want to delete the session "${sessionLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) {
    return;
  }

  try {
    await classSessionApi.delete(id);
    notify.success("Session deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>