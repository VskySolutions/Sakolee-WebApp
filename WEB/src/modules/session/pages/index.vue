<template>
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Sessions' }]"
      :search="search"
      show-search
      search-placeholder="Search session name"
      show-add
      add-label="Create Session"
      show-back
      @update:search="search = $event"
      @add="openCreatePage"
      @back="$router.back()"
    />

    <!-- Core Data Table Grid Component: Handles server-side pagination, sorting, row selection, and dataset display -->
    <app-data-table
      page-key="sessions"
      :row-key="(row) => row.sessionId || row.SessionId || row.id || row.Id"
      title="All Sessions"
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
      <!-- Bulk Actions Slot: Provides batch deletion functionality for selected table rows -->
      <template #bulk-actions="{ selected: sel }">
        <q-btn flat dense no-caps color="negative" label="Delete Selected" @click="bulkDelete(sel)" />
      </template>

      <!-- Row Actions Slot: Renders individual record operations including view details, editing, and deletion -->
      <template #body-cell-actions="cell">
        <q-td :props="cell" class="text-right" style="padding-right: 50px !important;">
          <!-- View Action: Triggers navigation to the session detail/view page -->
          <q-btn flat round dense color="primary" icon="o_visibility" @click="viewRecord(cell.row)">
            <q-tooltip>View Details</q-tooltip>
          </q-btn>
          <!-- Edit Action: Extracts unique identifier and navigates to the form page in update mode -->
          <q-btn flat round dense color="primary" icon="o_edit" @click="editRecord(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <!-- Delete Action: Triggers individual soft deletion workflow for the selected entity -->
          <q-btn flat round dense color="negative" icon="o_delete" @click="deleteRecord(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";

import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";

const router = useRouter();
const notify = useNotify();
const { confirm } = useConfirm();

// Data Grid Columns Definition Schema: Maps session properties with multi-case fallback handlers
const columns = computed(() => [
  { 
    name: "id", 
    label: "ID", 
    field: (r) => r.sessionId || r.SessionId || r.id || r.Id, 
    align: "left", 
    sortable: true 
  },
  { 
    name: "sessionName", 
    label: "Session Name", 
    field: (r) => r.sessionName || r.SessionName || r.name || r.Name, 
    align: "left", 
    sortable: true 
  },
  { 
    name: "createdOn", 
    label: "Created Date", 
    field: (r) => r.createdOn || r.CreatedOn, 
    align: "left", 
    sortable: true,
    format: (val) => {
      if (!val) return '-';
      const date = new Date(val);
      return isNaN(date.getTime()) ? String(val) : date.toLocaleString();
    }
  },
  { 
    name: "actions", 
    label: "Actions", 
    field: "actions", 
    align: "right", 
    style: "padding-right: 50px !important;", 
    headerStyle: "padding-right: 70px !important;" 
  }
]);

// List Table Composable Integration: Manages server-side data synchronization, pagination state, and query parameters
const { 
    rows, 
    loading, 
    totalRecords, 
    selected, 
    search, 
    pagination, 
    load, 
    onRequest 
} = useListTable({
    pageKey: "sessions",
    fetcher: ({ page, limit, sortBy, descending }) => {
        return classSessionApi.list({
            page,
            limit,
            sortBy,
            descending,
            search: search.value || undefined
        }).then((r) => ({ 
            data: r?.data?.items || r?.items || r?.data || [], 
            total: r?.data?.totalCount || r?.totalRecords || r?.total || 0 
        }));
    },
    onError: (err) => notify.error(getApiErrorMessage(err))
});

// Debounced Reload Handler: Optimizes network requests by throttling search input changes
const reload = debounce(() => { 
  pagination.value.page = 1; 
  load(); 
}, 300);

// Watcher for search keyword changes to reload the data grid table dynamically
watch(search, reload);

// Lifecycle Hook: Initial data load on component mount
onMounted(() => {
  load();
});

/**
 * Redirects the user interface to the dedicated session creation view.
 */
const openCreatePage = () => {
  router.push({ name: 'session_create' });
};

/**
 * Navigates to the detailed view page for a specific session record using fallback primary keys.
 * @param {Object} row - Target entity row data instance.
 */
const viewRecord = (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  router.push({ name: 'session_detail', params: { id } });
};

/**
 * Navigates to the form component in editing mode by passing the extracted record identifier via route query parameters.
 * @param {Object} row - Target entity row data instance.
 */
const editRecord = (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }
  router.push({ name: 'session_create', query: { id } });
};

/**
 * Prompts user confirmation and dispatches a delete request for an individual session record.
 * @param {Object} row - Target entity row data instance.
 */
const deleteRecord = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) return;

  const sessionLabel = row.sessionName || row.SessionName || row.name || row.Name || 'this session';
  const ok = await confirm({ title: "Delete Session", message: `Are you sure you want to delete "${sessionLabel}"?`, type: "danger" });
  if (!ok) return;

  try {
    await classSessionApi.delete(id);
    notify.success("Session deleted successfully");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Handles batch deletion operations for multiple selected session records concurrently.
 * @param {Array} sel - Array of selected entity row objects.
 */
const bulkDelete = async (sel) => {
  if (!sel.length) return;
  const ok = await confirm({ title: "Delete Selected", message: `Are you sure you want to delete ${sel.length} session(s)?`, type: "danger" });
  if (!ok) return;

  try {
    await Promise.all(sel.map(r => classSessionApi.delete(r.sessionId || r.SessionId || r.id || r.Id)));
    notify.success("Selected sessions deleted successfully");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>