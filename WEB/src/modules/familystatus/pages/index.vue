<template>
  <q-page padding>
    <!-- Page Header: Renders breadcrumb navigation, live search input, filter trigger, and primary actions -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Family Statuses' }]"
      :search="search"
      show-search
      search-placeholder="Search family status name"
      show-filters
      :filter-count="filterChips.length"
      show-add
      add-label="Create Family Status"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreateDialog"
      @back="$router.back()"
    />

    <!-- Filter Drawer: Column-level filtering and soft-delete toggle -->
    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-column-filters v-model="filters" :columns="filterableColumns" />
      <q-toggle
        v-if="canManageDeleted" v-model="showDeleted" label="Show deleted?" dense class="q-mt-md"
      />
    </app-filter-drawer>

    <!-- Core Data Table Grid: Server-side pagination, sorting, row selection, and batch processing -->
    <app-data-table
      page-key="family-status"
      row-key="familyStatusId"
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
      <!-- Slot: Contextual Bulk Actions Toolbar for Multi-select Operations -->
      <template #bulk-actions="{ selected: sel }">
        <q-btn flat dense no-caps color="negative" label="Delete Selected" @click="bulkDelete(sel)" />
      </template>

      <!-- Slot: Inline Row-level Action Controllers (View, Edit, Soft-Delete) -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" :to="{ name: 'family_status_detail', params: { id: cell.row.familyStatusId } }">
            <q-tooltip>View / Manage</q-tooltip>
          </q-btn>
          <q-btn flat round dense color="primary" icon="o_edit" @click="openEditDialog(cell.row.familyStatusId)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
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

    <!-- Side-Drawer Dialog: Create / Update Form Modal -->
    <q-dialog v-model="dialogOpen" position="right" maximized>
      <q-card class="column full-height" style="width: 500px; max-width: 100vw;">
        
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Family Status' : 'Create Family Status' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Scrollable Body Content & Reactive Form Container -->
        <q-card-section class="col q-pa-md scroll">
          <q-form ref="formRef" greedy @submit.prevent="submitForm">
            
            <!-- Foreign Key Selection: Associated Tenant Lookup (Visible only for Platform/Super Admins) -->
            <app-select
              v-if="canChooseTenant"
              v-model="form.tenantId"
              :options="tenantOptions"
              :loading="loadingTenants"
              label="Tenant"
              required
              emit-value
              map-options
              class="q-mb-md"
              :rules="[(v) => !!v || 'Tenant is required']"
            />

            <!-- Primary Entity Property: Family Status Name -->
            <app-text-field
              v-model="form.name"
              label="Status Name"
              required
              class="q-mb-md"
              :rules="[(v) => !!v || 'Status name is required']"
            />
          </q-form>
        </q-card-section>

        <!-- Dialog Footer Action Triggers -->
        <q-card-actions align="right" class="q-pa-md bg-grey-2">
          <q-btn flat label="Cancel" color="grey" v-close-popup />
          <q-btn unelevated color="primary" :label="editing ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";

import { api, familyStatusApi, getApiErrorMessage, EntityType } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useTenantOptions } from "composables/useTenantOptions";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";

// Composables & Routing Initialization
const router = useRouter();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { canChooseTenant, activeTenantId, tenantOptions, loadingTenants, loadTenants } = useTenantOptions();

// Modal Presentation and Mutation Flags
const dialogOpen = ref(false);
const editingId = ref(null);
const editing = ref(false);
const saving = ref(false);
const formRef = ref(null);

// Form Entity Binding Object (Payload Model)
const form = reactive({ 
  tenantId: null,
  name: "", 
});

// Tenant dropdown filter options for platform/super admins
const tenantFilterOptions = computed(() =>
  (canChooseTenant.value && tenantOptions.value.length ? tenantOptions.value : null));

// Data Grid Column Configuration Schema with server-side column filtering support
const columns = computed(() => [
  { name: "familyStatusId", label: "ID", field: "familyStatusId", align: "left", sortable: true, default: true, filterable: false },
  { name: "name", label: "Status Name", field: "name", align: "left", sortable: true, default: true, filterable: false },
  { 
    name: "tenantName", 
    label: "Tenant", 
    field: (row) => row.tenantName || row.tenant_name || row.tenantId, 
    format: (val, row) => {
      const tenantId = row.tenantId || row.tenantid;
      const found = tenantOptions.value.find((t) => t.value === tenantId);
      return found ? found.label : (row.tenantName || row.tenant_name || tenantId || '-');
    },
    align: "left", 
    sortable: true, 
    default: true,
    ...(tenantFilterOptions.value ? { filterOptions: tenantFilterOptions.value } : { filterable: false })
  },
  { 
    name: "createdOn", 
    label: "Created On", 
    field: (row) => {
      if (!row) return '-';
      const key = Object.keys(row).find(k => k.toLowerCase() === 'createdon' || k.toLowerCase() === 'createdat');
      return key ? row[key] : (row.CreatedOn || row.createdOn || row.createdAt || '-');
    }, 
    align: "left", 
    sortable: true, 
    default: true,
    filterable: false,
    format: (val) => {
      if (!val || val === '-') return '-';
      const date = new Date(val);
      return isNaN(date.getTime()) ? String(val) : date.toLocaleString(); 
    }
  },
  { name: "actions", label: "Actions", field: "actions", align: "left", filterable: false }
]);

// Server-side List Management Composable integrated with column filters
const { 
    rows, 
    loading, 
    totalRecords, 
    selected, 
    search, 
    filterOpen,
    pagination, 
    load, 
    onRequest 
} = useListTable({
    pageKey: "family-status",
    fetcher: async ({ page, limit, sortBy, descending }) => {
        const currentTenantId = filters.tenantName || activeTenantId.value;

        // Seedha global api instance use karenge taaki header aur query params dono confirm ho jayein
        const response = await api.get('/api/admin/family-statuses', {
            params: {
                page,
                limit,
                sortBy: sortBy || undefined,
                descending: descending ? true : undefined,
                search: search.value || undefined,
                tenantId: currentTenantId || undefined
            },
            headers: currentTenantId ? { 'X-Site-Id': currentTenantId } : {}
        });

        return { 
            data: response?.data?.data || response?.data, 
            total: response?.data?.meta?.totalRecords || response?.data?.totalRecords || 0 
        };
    },
    onError: (err) => notify.error(getApiErrorMessage(err))
});

// Column Filters Setup (Server-side integration)
const { filters, filterableColumns, filterChips, removeFilter, clearFilters } = useColumnFilters(columns, rows, { server: true });

// Debounced reload handler resetting pagination index on search or filter updates
const reload = debounce(() => { 
  pagination.value.page = 1; 
  load(); 
}, 300);


// Watch for active tenant changes and reload table data automatically
watch(activeTenantId, () => {
  reload();
});

// Component Mount Hook: Load tenant options if user has platform privileges
onMounted(() => {
  if (canChooseTenant.value) {
    loadTenants();
  }
});

/**
 * Initializes state context and opens the drawer dialog in Creation mode.
 */
const openCreateDialog = async () => {
  editingId.value = null;
  editing.value = false;
  form.name = "";
  
  if (canChooseTenant.value) {
    await loadTenants();
    form.tenantId = activeTenantId.value || null;
  } else {
    form.tenantId = activeTenantId.value;
  }
  
  dialogOpen.value = true;
};

/**
 * Hydrates target record metadata and opens the drawer dialog in Update mode.
 * @param {string|number} id - Target entity primary key identifier.
 */
const openEditDialog = async (id) => {
  editingId.value = id;
  editing.value = true;
  dialogOpen.value = true;
  
  try {
    const response = await api.get(`/api/admin/family-statuses/${id}`);
    const item = response?.data?.data || response?.data;
    
    if (item) {
      form.tenantId = item.tenantId || item.tenantid || null;
      form.name = item.name || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Validates form integrity and executes either a POST (Create) or PUT (Update) transaction.
 */
const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
      tenantId: form.tenantId,
      name: form.name
    };

    if (editing.value && editingId.value) {
      await familyStatusApi.update(editingId.value, payload);
      notify.success("Family status updated successfully.");
    } else {
      await familyStatusApi.create(payload);
      notify.success("Family status created successfully.");
    }

    dialogOpen.value = false;
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};

/**
 * Prompts user confirmation and executes a safe soft-delete request for an individual record.
 * @param {Object} row - Entity row model instance.
 */
const deleteRecord = async (row) => {
  const ok = await confirm({
    title: "Delete family status",
    message: `Are you sure you want to delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;

  try {
    await familyStatusApi.delete(row.familyStatusId);
    notify.success("Family status deleted successfully.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Handles batch processing for multi-selected records with confirmation safeguards.
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
    await Promise.all(sel.map((r) => familyStatusApi.delete(r.familyStatusId)));
    notify.success("Selected statuses deleted successfully.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>