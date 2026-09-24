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
      @add="openCreateDialog"
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
          <!-- View Action: Triggers right-side drawer view modal -->
          <q-btn flat round dense color="primary" icon="o_visibility" @click="viewRecord(cell.row)">
            <q-tooltip>View Details</q-tooltip>
          </q-btn>
          <!-- Edit Action: Extracts unique identifier and opens form drawer in update mode -->
          <q-btn flat round dense color="primary" icon="o_edit" @click="openEditDialog(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <!-- Delete Action: Triggers individual soft deletion workflow for the selected entity -->
          <q-btn flat round dense color="negative" icon="o_delete" @click="deleteRecord(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Side-Drawer Dialog Component: Create / Update Form Modal -->
    <q-dialog v-model="dialogOpen" position="right">
      <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
        
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Session' : 'Create Session' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Dialog Scrollable Body Content & Reactive Form Container -->
        <q-card-section class="col q-pa-md scroll">
          <!-- Loading Spinner Overlay during fetch -->
          <div v-if="formLoading" class="row flex-center q-pa-xl">
            <q-spinner color="primary" size="40px" />
          </div>

          <q-form v-else ref="formRef" greedy @submit.prevent="submitForm">
            <!-- Primary Entity Property: Session Name -->
            <app-text-field
              v-model="form.sessionName"
              label="Session Name"
              required
              class="q-mb-md"
              :rules="[(v) => !!v || 'Session name is required']"
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

    <!-- Side-Drawer Dialog Component: View Session Details Modal -->
    <!-- Center Dialog Component: View Session Details Modal -->
 <!-- Side-Drawer Dialog Component: View Session Details Modal -->
  <q-dialog v-model="viewDialogOpen" position="right">
    <q-card class="column shadow-24 rounded-borders" style="width: 420px; max-width: 90vw; height: auto; max-height: 85vh;">
      
      <!-- Dialog Header Banner -->
      <q-card-section class="row items-center justify-between bg-primary text-white q-px-md q-py-sm">
        <div class="text-h6 text-weight-bold row items-center q-gutter-sm">
          <q-icon name="o_visibility" size="22px" />
          <div>Session Details</div>
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
          <!-- Field: Session Name -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Session Name:</div>
            <div class="col-7 text-dark">{{ viewForm.sessionName || '-' }}</div>
          </div>
          <q-separator />

          <!-- Field: Created Date -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Created Date:</div>
            <div class="col-7 text-dark">{{ viewForm.createdOn || '-' }}</div>
          </div>
        </template>
      </q-card-section>

      <!-- Dialog Footer Action Triggers -->
      <q-card-actions align="right" class="q-pa-sm bg-grey-1">
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

import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppTextField from "components/common/AppTextField.vue";

const router = useRouter();
const notify = useNotify();
const { confirm } = useConfirm();

// Component modal presentation flags and entity tracking references for Drawer
const dialogOpen = ref(false);
const editingId = ref(null);
const editing = ref(false);
const saving = ref(false);
const formLoading = ref(false);
const formRef = ref(null);

// View Drawer state variables and data bindings
const viewDialogOpen = ref(false);
const viewLoading = ref(false);
const viewForm = reactive({
  sessionName: "",
  createdOn: ""
});

// Form payload data model bindings
const form = reactive({ 
  sessionName: "" 
});

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
            sortBy: sortBy || "createdOn",
            descending: descending !== undefined ? descending : true,
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
 * Initializes state and opens the drawer dialog in creation mode.
 */
const openCreateDialog = () => {
  editingId.value = null;
  editing.value = false;
  form.sessionName = "";
  dialogOpen.value = true;
};

/**
 * Hydrates target record metadata and opens the drawer dialog in update mode.
 * @param {Object} row - Target entity row data instance.
 */
const openEditDialog = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }

  editingId.value = id;
  editing.value = true;
  dialogOpen.value = true;
  formLoading.value = true;
  
  try {
    const response = await classSessionApi.get(id);
    const item = response?.data?.data || response?.data || response;
    
    if (item) {
      form.sessionName = item.sessionName || item.SessionName || item.name || item.Name || "";
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    formLoading.value = false;
  }
};

/**
 * Opens the right-side drawer modal and fetches session details for viewing.
 * @param {Object} row - Target entity row data instance.
 */
const viewRecord = async (row) => {
  const id = row.sessionId || row.SessionId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }

  viewDialogOpen.value = true;
  viewLoading.value = true;

  try {
    const response = await classSessionApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewForm.sessionName = item.sessionName || item.SessionName || item.name || item.Name || "-";
      
      const rawDate = item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawDate) {
        const date = new Date(rawDate);
        viewForm.createdOn = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        viewForm.createdOn = "-";
      }
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

/**
 * Validates form constraints and dispatches either a POST (Create) or PUT (Update) API transaction.
 */
const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
      Name: form.sessionName,
      sessionName: form.sessionName,
      active: true
    };

    if (editing.value && editingId.value) {
      await classSessionApi.update(editingId.value, payload);
      notify.success("Session updated successfully.");
    } else {
      await classSessionApi.create(payload);
      notify.success("Session created successfully.");
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