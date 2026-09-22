<template>
  <q-page padding>
    <!-- Page Header: Manages breadcrumbs, live search input, creation trigger, and navigation -->
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

    <!-- Core Data Table Grid: Handles server-side pagination, sorting, row selection, and batch actions -->
    <app-data-table
      page-key="family-status"
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
      <!-- Slot: Contextual Bulk Actions Toolbar for Multi-select Operations -->
      <template #bulk-actions="{ selected: sel }" >
        <q-btn flat dense no-caps color="negative" label="Delete Selected" @click="bulkDelete(sel)" />
      </template>

      <!-- Slot: Inline Row-level Action Controllers (View, Edit, Delete) -->
      <template #body-cell-actions="cell">
        <q-td :props="cell" class="text-right" style="padding-right: 50px !important;">
          <!-- Fixed View Action: Uses a method handler to safely extract ID and navigate -->
          <q-btn flat round dense color="primary" icon="o_visibility" @click="viewRecord(cell.row)">
            <q-tooltip>View / Manage</q-tooltip>
          </q-btn>
          <q-btn flat round dense color="primary" icon="o_edit" @click="openEditDialog(cell.row.familyStatusId || cell.row.FamilyStatusId || cell.row.id)">
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
    <q-dialog v-model="dialogOpen" position="right">
      <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
        
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Family Status' : 'Create Family Status' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Scrollable Body Content & Reactive Form Container -->
        <q-card-section class="col q-pa-md scroll">
          <q-form ref="formRef" greedy @submit.prevent="submitForm">
            
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
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useTenantOptions } from "composables/useTenantOptions";
import { useTenantScope } from "composables/useTenantScope";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

// Initialize core routing, notifications, and tenant management composables
const router = useRouter();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { tenantOptions, loadTenants } = useTenantOptions();
const { refreshTenants } = useTenantScope();

// Component modal presentation flags and entity reference trackers
const dialogOpen = ref(false);
const editingId = ref(null);
const editing = ref(false);
const saving = ref(false);
const formRef = ref(null);

// Form payload data model binding structure containing only the status name field
const form = reactive({ 
  name: "" 
});

// Data Grid Column Configuration Schema with enhanced fallback handling for Tenant Name display
const columns = computed(() => [
  { name: "familyStatusId", label: "ID", field: "familyStatusId", align: "left", sortable: true, default: true },
  { name: "name", label: "Status Name", field: "name", align: "left", sortable: true, default: true },
  { 
    name: "tenantName", 
    label: "Tenant Name", 
    field: (row) => row.tenantName || row.tenant_name || row.tenant?.name || row.tenantId, 
    format: (val, row) => {
      // Check if tenant object or direct name properties are present
      if (row.tenantName) return row.tenantName;
      if (row.tenant_name) return row.tenant_name;
      if (row.tenant?.name) return row.tenant.name;

      const tenantId = row.tenantId || row.tenantid || row.TenantId;
      if (!tenantId) return '-';

      // Match against loaded tenant options collection
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

// Server-side List Management Composable configured to fetch and display records across all tenants
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
    fetcher: ({ page, limit, sortBy, descending }) => {
        return familyStatusApi.list({
            page,
            limit,
            sortBy,
            descending,
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
 * Handles navigation to the detailed view page for a specific record.
 * @param {Object} row - Entity row model instance.
 */
const viewRecord = (row) => {
  const id = row.familyStatusId || row.FamilyStatusId || row.id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  router.push({ name: 'family_status_detail', params: { id } });
};

/**
 * Initializes state context and opens the drawer dialog in creation mode.
 */
const openCreateDialog = () => {
  editingId.value = null;
  editing.value = false;
  form.name = "";
  dialogOpen.value = true;
};

/**
 * Hydrates target record metadata and opens the drawer dialog in edit/update mode.
 * @param {string|number} id - Target entity primary key identifier.
 */
const openEditDialog = async (id) => {
  if (!id) {
    notify.error("Invalid record identifier.");
    return;
  }

  editingId.value = id;
  editing.value = true;
  dialogOpen.value = true;
  
  try {
    const response = await api.get(`/api/admin/family-statuses/${id}`);
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.name = item.name || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/**
 * Validates form integrity constraints and executes either a POST (Create) or PUT (Update) API transaction.
 */
const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
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
    refreshTenants();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
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