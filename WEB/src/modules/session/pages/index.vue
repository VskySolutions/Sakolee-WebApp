<template>
<<<<<<< Updated upstream
  <!--
    ============================================================
    Sessions Index Page Component
    ============================================================
    Manages session listings, filtering, server-side data grid operations,
    and side-drawers for viewing, creating, and editing sessions.
  -->
=======
>>>>>>> Stashed changes
  <q-page padding>
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Sessions' }
      ]"
      title="Class-Sessions"
      description="Manage your all class sessions here."
      :search="search"
      show-search
      search-placeholder="Search Class-Sessions"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Class-Session"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

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
      <!-- Session Name Column with Deleted Indicator -->
      <template #body-cell-sessionName="cell">
        <q-td :props="cell" :class="{ 'text-strike text-grey': cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted }">
          {{ cell.row.sessionName || cell.row.SessionName || cell.row.name || cell.row.Name }}
          <q-badge v-if="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted" color="negative" class="q-ml-sm" dense>
            Deleted
          </q-badge>
        </q-td>
      </template>

      <!-- Interactive Status Toggle Column Slot -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <div class="row items-center q-gutter-x-sm">
            <q-toggle
              :model-value="cell.row.isActive ?? cell.row.IsActive ?? cell.row.active ?? cell.row.Active ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted"
            />
            <span :class="(cell.row.isActive ?? cell.row.IsActive ?? cell.row.active ?? cell.row.Active ?? true) ? 'text-positive' : 'text-grey'">
              {{ (cell.row.isActive ?? cell.row.IsActive ?? cell.row.active ?? cell.row.Active ?? true) ? "Active" : "Inactive" }}
            </span>
          </div>
        </q-td>
      </template>

      <!-- Row Actions Slot with Disabled check for deleted records -->
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
            :disabled="cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted"
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
            v-if="!(cell.row.deleted || cell.row.Deleted || cell.row.isDeleted || cell.row.IsDeleted)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Drawer Component -->
    <session-form
      v-model="formOpen"
      :editing-id="editingId"
      :initial-data="selectedRow"
      @saved="handleSaved"
    />

    <!-- View Details Drawer Component -->
    <session-view
      v-model="viewOpen"
      :record-id="viewRecordId"
    />
  </q-page>
</template>

<script setup>
<<<<<<< Updated upstream
import { ref, reactive, watch, onMounted } from "vue";
=======
import { ref, watch, onMounted } from "vue";
>>>>>>> Stashed changes
import { debounce } from "quasar";

import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";

import SessionForm from "src/modules/session/components/create_edit_session.vue";
import SessionView from "src/modules/session/components/view_session.vue";

// State variables for managing the list of sessions
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();

// Function to format date values for display
const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

// Define the columns for the sessions data table
const columns = [
  {
    name: "sessionName",
    label: "Session Name",
    field: (r) => r.sessionName || r.SessionName || r.name || r.Name,
    align: "left",
    sortable: true,
    default: true,
    filterable: true
  },
  {
    name: "createdBy",
    label: "Created By",
    field: (r) => r.createdBy || r.CreatedBy || r.created_by || "—",
    align: "left",
    sortable: true,
    default: false,
    filterable: false
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: (r) => r.createdOnUtc || r.CreatedOnUtc || r.created_on_utc || r.createdOn || r.CreatedOn || r.created_on || null,
    align: "left",
    sortable: true,
    default: false,
    format: (val) => formatDate(val),
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
    format: (val) => formatDate(val),
    filterable: false
  },
 {
    name: "active",
    label: "Status",
    field: (r) => r.isActive ?? r.IsActive ?? r.active ?? r.Active ?? true,
    align: "center",
    sortable: true,
    default: true,
    filterable: false
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left",
    filterable: false
  }
];

// Reactive state for managing the filter drawer visibility
const filterOpen = ref(false);

// Reactive state for managing the search input
const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "sessions",
  pagination: {
    sortBy: "createdOnUtc",
    descending: true,
    page: 1,
    rowsPerPage: 20
  },

  // Fetcher function to retrieve session data from the API
  fetcher: ({ page, limit, sortBy, descending }) => {
    let mappedSortBy = sortBy;
    if (sortBy === "sessionName") mappedSortBy = "Name";
    else if (sortBy === "createdBy") mappedSortBy = "CreatedBy";
    else if (sortBy === "createdOnUtc") mappedSortBy = "CreatedOn";
    else if (sortBy === "updatedBy") mappedSortBy = "UpdatedBy";
    else if (sortBy === "updatedOnUtc") mappedSortBy = "UpdatedOn";

    // Construct query parameters for the API request
    const queryParams = {
      page,
      limit,
      search: search.value || undefined,
      sortBy: mappedSortBy || "CreatedOn",
      descending: descending ?? true,
      showDeleted: showDeleted.value,
      includeDeleted: showDeleted.value
    };

    // Fetch the session list from the API and return the data and total count
    return classSessionApi.list(queryParams).then((response) => {
      const items = response?.data?.items || response?.items || response?.data || [];
      const total = response?.data?.total || response?.total || items.length;
      return {
        data: items,
        total: total
      };
    });
  },

  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Use the useColumnFilters composable to manage column-based filtering for the sessions data table
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

// Debounced reload function to reset pagination and reload data when search input changes
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);
watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

onMounted(() => {
  load();
});

// Create / Edit Drawer states
const formOpen = ref(false);
const editingId = ref(null);
const selectedRow = ref(null);

// Function to open the create session drawer
const openCreate = () => {
  editingId.value = null;
  selectedRow.value = null;
  formOpen.value = true;
};

// Function to open the edit session drawer with the selected row's data
const openEdit = (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  editingId.value = id;
  selectedRow.value = row;
  formOpen.value = true;
};

// Function to update the active status inline via toggle
const updateStatus = async (row, newStatus) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) return;

  const originalStatus = row.isActive ?? row.IsActive ?? row.active ?? row.Active ?? true;
  
  if (row.active !== undefined) row.active = newStatus;
  if (row.Active !== undefined) row.Active = newStatus;
  if (row.isActive !== undefined) row.isActive = newStatus;
  if (row.IsActive !== undefined) row.IsActive = newStatus;

  try {
    const payload = {
      name: row.sessionName || row.SessionName || row.name || row.Name,
      isActive: newStatus,
      active: newStatus,
      isDeleted: row.isDeleted || row.IsDeleted || false
    };
    await classSessionApi.update(id, payload);
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    if (row.active !== undefined) row.active = originalStatus;
    if (row.Active !== undefined) row.Active = originalStatus;
    if (row.isActive !== undefined) row.isActive = originalStatus;
    if (row.IsActive !== undefined) row.IsActive = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};
// Function to handle the saved event from the create/edit drawer and reload the sessions list
const handleSaved = async () => {
  pagination.value.page = 1;
  await load();
};

// View Drawer states
const viewOpen = ref(false);
const viewRecordId = ref(null);

// Function to open the view session drawer with the selected row's data
const openView = (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  viewRecordId.value = id;
  viewOpen.value = true;
};

// Function to remove a session with confirmation and reload the sessions list
const removeSession = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) return;

  const sessionLabel = row.sessionName || row.SessionName || row.name || row.Name || "this session";
  const ok = await confirm({
    title: "Delete session",
    message: `Are you sure you want to delete the session "${sessionLabel}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await classSessionApi.delete(id);
    notify.success("Session deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
