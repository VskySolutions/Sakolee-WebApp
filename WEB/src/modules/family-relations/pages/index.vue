<template>
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Family Relations' }]"
      :search="search"
      show-search
      search-placeholder="Search family relation name"
      show-add
      add-label="Create Family Relation"
      show-back
      @update:search="search = $event"
      @add="openCreateDialog"
      @back="$router.back()"
    />

    <!-- Core Data Table Grid Component: Handles server-side pagination, sorting, row selection, and dataset display -->
    <app-data-table
      page-key="family-relation-v"
      :row-key="(row) => row.familyRelationId || row.FamilyRelationId || row.id"
      title="All Family Relations"
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
      v-if="canManageDeleted" :entity-type="EntityType.FamilyRelation" :show="showDeleted" @restored="load"
    />

    <!-- Create / Edit Form Drawer Component -->
    <family-relation-form-drawer
      v-model="formDrawerOpen"
      :editing-id="editingId"
      @saved="handleSaved"
    />

    <!-- Side-Drawer Dialog Component: View Family Relation Details Modal -->
    <q-dialog v-model="viewDrawerOpen" position="right">
      <q-card class="column shadow-24 rounded-borders" style="width: 450px; max-width: 90vw; height: 100vh; max-height: 100vh;">
        
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center justify-between bg-primary text-white q-px-md q-py-sm">
          <div class="text-h6 text-weight-bold row items-center q-gutter-sm">
            <q-icon name="o_visibility" size="22px" />
            <div>Family Relation Details</div>
          </div>
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Scrollable Body Content -->
        <q-card-section class="col q-pa-md q-gutter-md scroll">
          <!-- Loading Spinner Overlay during fetch operations -->
          <div v-if="viewLoading" class="row flex-center q-pa-xl">
            <q-spinner color="primary" size="40px" />
          </div>

          <template v-else>
            <!-- Field: Family Relation Name -->
            <div class="row items-center">
              <div class="col-5 text-weight-bold text-grey-7">Relation Name:</div>
              <div class="col-7 text-dark">{{ viewForm.name || '-' }}</div>
            </div>
            <q-separator />

            <!-- Field: Active Status -->
            <div class="row items-center">
              <div class="col-5 text-weight-bold text-grey-7">Active Status:</div>
              <div class="col-7">
                <q-chip dense :color="viewForm.active ? 'positive' : 'grey'" text-color="white">
                  {{ viewForm.active ? 'Active' : 'Inactive' }}
                </q-chip>
              </div>
            </div>
            <q-separator />

            <!-- Field: Tenant Name -->
            <div class="row items-center">
              <div class="col-5 text-weight-bold text-grey-7">Tenant Name:</div>
              <div class="col-7 text-dark">{{ viewForm.tenantName || '-' }}</div>
            </div>
            <q-separator />

            <!-- Field: Created Date -->
            <div class="row items-center">
              <div class="col-5 text-weight-bold text-grey-7">Created Date:</div>
              <div class="col-7 text-dark">{{ viewForm.createdOnUtc || '-' }}</div>
            </div>
          </template>
        </q-card-section>

        <!-- Dialog Footer Action Triggers -->
        <q-card-actions align="right" class="q-pa-md bg-grey-1">
          <q-btn flat label="Close" color="primary" v-close-popup />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";

import { familyRelationApi, getApiErrorMessage, EntityType } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useTenantOptions } from "composables/useTenantOptions";
import { useTenantScope } from "composables/useTenantScope";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppListHeader from "components/common/AppListHeader.vue";

// Import Form Drawer Component
import FamilyRelationFormDrawer from "src/modules/family-relations/components/create_edit.vue";

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

// View Drawer state variables and data bindings
const viewLoading = ref(false);
const viewForm = reactive({
  name: "",
  active: true,
  tenantName: "",
  createdOnUtc: ""
});

// Data Grid Column Configuration Schema with enhanced fallback handling for Tenant Name and Creation Date display
const columns = computed(() => [
  { 
    name: "familyRelationId", 
    label: "ID", 
    field: (row) => {
      if (!row) return '-';
      return row.familyRelationId || row.FamilyRelationId || row.family_relation_id || row.id || row.Id || '-';
    }, 
    align: "left", 
    sortable: true, 
    default: true 
  },
  { name: "name", label: "Relation Name", field: "name", align: "left", sortable: true, default: true },
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
  { name: "actions", label: "Actions", field: "actions", align: "right", style: "padding-right: 50px !important;", headerStyle: "padding-right: 70px !important;" }
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
    pageKey: "family-relation",
    defaultSortBy: "createdOnUtc",
    defaultDescending: true,
    fetcher: ({ page, limit, sortBy, descending }) => {
        return familyRelationApi.list({
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
  const id = row.familyRelationId || row.FamilyRelationId || row.id;
  if (!id) {
    notify.error("Invalid record identifier.");
    return;
  }
  editingId.value = id;
  formDrawerOpen.value = true;
};

/**
 * Opens the view details right drawer and fetches record info.
 * @param {Object} row - Target entity row data instance.
 */
const openViewDrawer = async (row) => {
  const id = row.familyRelationId || row.FamilyRelationId || row.id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }

  viewDrawerOpen.value = true;
  viewLoading.value = true;

  try {
    const response = await familyRelationApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewForm.name = item.name || item.Name || "-";
      viewForm.active = item.active !== undefined ? item.active : (item.Active !== undefined ? item.Active : true);
      
      // Resolve Tenant Name
      const tId = item.tenantId || item.TenantId;
      const foundTenant = tenantOptions.value.find((t) => t.value === tId || t.id === tId);
      viewForm.tenantName = item.tenantName || item.tenant_name || (foundTenant ? foundTenant.label : (tId || "-"));

      // Format Date
      const rawDate = item.createdOnUtc || item.CreatedOnUtc || item.createdOn || item.CreatedOn;
      if (rawDate) {
        const date = new Date(rawDate);
        viewForm.createdOnUtc = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        viewForm.createdOnUtc = "-";
      }
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
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
  const id = row.familyRelationId || row.FamilyRelationId || row.id;
  if (!id) {
    notify.error("Invalid record identifier for deletion.");
    return;
  }

  const ok = await confirm({
    title: "Delete family relation",
    message: `Are you sure you want to delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;

  try {
    await familyRelationApi.delete(id);
    notify.success("Family relation deleted successfully.");
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
    title: "Delete selected relations",
    message: `Are you sure you want to delete ${sel.length} relation(s)?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;

  try {
    await Promise.all(sel.map((r) => {
      const id = r.familyRelationId || r.FamilyRelationId || r.id;
      return familyRelationApi.delete(id);
    }));
    notify.success("Selected relations deleted successfully.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>