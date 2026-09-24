<template>
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Family Statuses' }]"
      :search="search"
      show-search
      search-placeholder="Search family status name"
      show-add
      add-label="Create Family Status"
      show-back
      @update:search="search = $event"
      @add="openCreateDialog"
      @back="$router.back()"
    />

    <!-- Core Data Table Grid Component: Handles server-side pagination, sorting, row selection, and dataset display -->
    <app-data-table
      page-key="family-status-v"
      :row-key="(row) => row.familyStatusId || row.FamilyStatusId || row.id"
      title="All Family Statuses"
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
          <!-- View Action: Triggers view drawer modal -->
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openViewDrawer(cell.row)">
            <q-tooltip>View Details</q-tooltip>
          </q-btn>
          <!-- Edit Action: Triggers form drawer in update mode -->
          <q-btn flat round dense color="primary" icon="o_edit" @click="openEditDialog(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <!-- Delete Action: Triggers individual deletion workflow -->
          <q-btn flat round dense color="negative" icon="o_delete" @click="deleteRecord(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Soft-Deleted Entities Management Panel -->
    <deleted-records-panel
      v-if="canManageDeleted" :entity-type="EntityType.FamilyStatus" :show="showDeleted" @restored="load"
    />

    <!-- Create / Edit Form Drawer Component -->
    <family-status-form-drawer
      v-model="formDrawerOpen"
      :editing-id="editingId"
      @saved="handleSaved"
    />

    <!-- View Details Drawer Component -->
    <family-status-view-drawer
      v-model="viewDrawerOpen"
      :record-id="viewingId"
    />
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";

import { familyStatusApi, getApiErrorMessage, EntityType } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useTenantOptions } from "composables/useTenantOptions";
import { useTenantScope } from "composables/useTenantScope";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppListHeader from "components/common/AppListHeader.vue";

// Import separate modular drawer components
import FamilyStatusFormDrawer from "src/modules/familystatus/components/create_edit_status.vue";
import FamilyStatusViewDrawer from "src/modules/familystatus/components/viewstatus.vue";

// Initialize core routing, notifications, and tenant management composables
const router = useRouter();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { tenantOptions, loadTenants } = useTenantOptions();
const { refreshTenants } = useTenantScope();

// Drawer visibility states and identifier trackers
const formDrawerOpen = ref(false);
const viewDrawerOpen = ref(false);
const editingId = ref(null);
const viewingId = ref(null);

// Data Grid Column Configuration Schema with enhanced fallback handling for Tenant Name and Creation Date display
const columns = computed(() => [
  { 
    name: "familyStatusId", 
    label: "ID", 
    field: (row) => {
      if (!row) return '-';
      return row.familyStatusId || row.FamilyStatusId || row.family_status_id || row.id || row.Id || '-';
    }, 
    align: "left", 
    sortable: true, 
    default: true 
  },
  { name: "name", label: "Status Name", field: "name", align: "left", sortable: true, default: true },
  { 
    name: "tenantName", 
    label: "Tenant Name", 
    field: (row) => row.tenantName || row.tenant_name || row.tenant?.name || row.tenantId, 
    format: (val, row) => {
      if (row.tenantName) return row.tenantName;
      if (row.tenant_name) return row.tenant_name;
      if (row.tenant?.name) return row.tenant.name;

      const tenantId = row.tenantId || row.tenantid || row.TenantId;
      if (!tenantId) return '-';

      const found = tenantOptions.value.find((t) => t.value === tenantId || t.id === tenantId);
      return found ? found.label : tenantId;
    },
    align: "left", 
    sortable: true, 
    default: true
  },
  { 
    name: "createdOnUtc", 
    label: "Created On", 
    field: (row) => {
      if (!row) return '-';
      const key = Object.keys(row).find(k => k.toLowerCase() === 'createdonutc' || k.toLowerCase() === 'createdon' || k.toLowerCase() === 'createdat');
      return key ? row[key] : (row.CreatedOnUtc || row.createdOnUtc || row.CreatedOn || row.createdOn || row.createdAt || '-');
    }, 
    align: "left", 
    sortable: true, 
    default: true,
    format: (val) => {
      if (!val || val === '-') return '-';
      const date = new Date(val);
      return isNaN(date.getTime()) ? String(val) : date.toLocaleString(); 
    }
  },
  { name: "actions", label: "Actions", field: "actions", align: "right" ,style: "padding-right: 50px !important;", headerStyle: "padding-right: 70px !important;"}
]);

// Server-side List Management Composable configured with default sorting to display newest created records at the top
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
    pageKey: "family-status",
    defaultSortBy: "createdOnUtc",
    defaultDescending: true,
    fetcher: ({ page, limit, sortBy, descending }) => {
        return familyStatusApi.list({
            page,
            limit,
            sortBy: sortBy || "createdOnUtc",
            descending: descending !== undefined ? descending : true,
            search: search.value || undefined,
            allTenants: true 
        }).then((r) => ({ 
            data: r?.data, 
            total: r?.meta?.totalRecords || r?.totalRecords || 0 
        }));
    },
    onError: (err) => notify.error(getApiErrorMessage(err))
});

// Debounced reload handler resetting pagination index upon search term modifications
const reload = debounce(() => { 
  pagination.value.page = 1; 
  load(); 
}, 300);

// Watch for search query input mutations
watch(search, reload);

// Load tenant options on mount and subsequently refresh table records
onMounted(async () => {
  await loadTenants();
  load();
});

/**
 * Opens the form drawer in creation mode.
 */
const openCreateDialog = () => {
  editingId.value = null;
  formDrawerOpen.value = true;
};

/**
 * Opens the form drawer in update/edit mode for a specific record.
 * @param {Object} row - Target entity row data instance.
 */
const openEditDialog = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id;
  if (!id) {
    notify.error("Invalid record identifier.");
    return;
  }
  editingId.value = id;
  formDrawerOpen.value = true;
};

/**
 * Opens the view details drawer for a specific record.
 * @param {Object} row - Target entity row data instance.
 */
const openViewDrawer = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  viewingId.value = id;
  viewDrawerOpen.value = true;
};

/**
 * Callback handler executed after a successful record creation or update operation.
 */
const handleSaved = (isNew) => {
  if (isNew) {
    pagination.value.page = 1;
    pagination.value.sortBy = "createdOnUtc";
    pagination.value.descending = true;
  }
  load();
  refreshTenants();
};

/**
 * Prompts user confirmation and executes a safe soft-delete request for an individual record entry.
 * @param {Object} row - Entity row model instance.
 */
const deleteRecord = async (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id;
  if (!id) {
    notify.error("Invalid record identifier for deletion.");
    return;
  }

  const ok = await confirm({
    title: "Delete family status",
    message: `Are you sure you want to delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;

  try {
    await familyStatusApi.delete(id);
    notify.success("Family status deleted successfully.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Handles batch processing operations for multi-selected records with confirmation safeguarding.
 * @param {Array} sel - Array of selected row objects.
 */
const bulkDelete = async (sel) => {
  if (!sel.length) return;
  const ok = await confirm({
    title: "Delete selected statuses",
    message: `Are you sure you want to delete ${sel.length} status(es)?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;

  try {
    await Promise.all(sel.map((r) => {
      const id = r.familyStatusId || r.FamilyStatusId || r.id;
      return familyStatusApi.delete(id);
    }));
    notify.success("Selected statuses deleted successfully.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>