<template>
  <q-page padding>
    <!-- Page Header Component: Manages breadcrumb navigation, live search inputs, and entity creation triggers -->
    <app-list-header
      :breadcrumbs="[{ label: 'Home', icon: 'o_home', to: '/' }, { label: 'Billing Methods' }]"
      :search="search"
      show-search
      search-placeholder="Search billing method name"
      show-add
      add-label="Create Billing Method"
      show-back
      @update:search="search = $event"
      @add="openCreateDrawer"
      @back="$router.back()"
    />

    <!-- Core Data Table Grid Component: Handles server-side pagination, sorting, row selection, and dataset display -->
    <app-data-table
      page-key="billing-methods"
      :row-key="(row) => row.billingMethodId || row.BillingMethodId || row.id || row.Id"
      title="All Billing Methods"
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
          <q-btn flat round dense color="primary" icon="o_visibility" @click="viewRecord(cell.row)">
            <q-tooltip>View Details</q-tooltip>
          </q-btn>
          <!-- Edit Action: Extracts unique identifier and opens the form drawer in update mode -->
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

    <!-- Create / Edit Right-Side Drawer Dialog Component -->
    <q-dialog v-model="drawerOpen" position="right" maximized persistent>
      <q-card class="column" style="width: 500px; max-width: 100vw; height: 500px; max-height: 70vh;">
        <!-- Dialog Header Banner -->
        <q-card-section class="row items-center q-pb-none bg-primary text-white">
          <div class="text-h6">{{ editing ? 'Edit Billing Method' : 'Create Billing Method' }}</div>
          <q-space />
          <q-btn icon="o_close" flat round dense @click="closeDrawer" />
        </q-card-section>

        <!-- Dialog Body Form Content Container -->
        <q-card-section class="col q-pa-md scroll">
          <!-- Loading Spinner Overlay during individual record fetch -->
          <div v-if="formLoading" class="row flex-center q-pa-xl">
            <q-spinner color="primary" size="40px" />
          </div>

          <q-form v-else ref="formRef" greedy @submit.prevent="submitForm">
            <!-- Billing Method Name Input Field -->
            <app-text-field
              v-model="form.name"
              label="Billing Method Name"
              required
              :error="!!nameDuplicateError"
              :error-message="nameDuplicateError"
              @update:model-value="nameDuplicateError = ''"
              class="q-mb-md"
              :rules="[(v) => !!v || 'Billing method name is required']"
            />

            <!-- Active Status Toggle Field -->
            <q-toggle
              v-model="form.active"
              label="Active"
              class="q-mb-md"
            />
          </q-form>
        </q-card-section>

        <!-- Dialog Footer Action Triggers -->
        <q-card-actions align="right" class="q-pa-md bg-grey-2">
          <q-btn flat label="Cancel" color="grey" @click="closeDrawer" />
          <q-btn unelevated color="primary" :label="editing ? 'Update' : 'Save'" :loading="saving" @click="submitForm" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- View Right-Side Drawer Component Integration -->
    <billing-method-view-drawer v-model="viewDrawerOpen" :record-id="viewRecordId" />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import { debounce } from "quasar";

import { billingMethodApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppTextField from "components/common/AppTextField.vue";
import BillingMethodViewDrawer from "src/modules/bankmethod/components/view_bankmethod.vue";

const router = useRouter();
const notify = useNotify();
const { confirm } = useConfirm();

// Drawer & Form State References
const drawerOpen = ref(false);
const editing = ref(false);
const editingId = ref(null);
const formLoading = ref(false);
const saving = ref(false);
const formRef = ref(null);

// View Drawer State References
const viewDrawerOpen = ref(false);
const viewRecordId = ref(null);

const nameDuplicateError = ref("");

// Form Payload Model Definition
const form = reactive({
  name: "",
  active: true
});

// Data Grid Columns Definition Schema
const columns = computed(() => [
  { 
    name: "id", 
    label: "ID", 
    field: (r) => r.billingMethodId || r.BillingMethodId || r.id || r.Id, 
    align: "left", 
    sortable: true 
  },
  { 
    name: "name", 
    label: "Billing Method Name", 
    field: (r) => r.name || r.Name || r.billingMethodName || r.BillingMethodName, 
    align: "left", 
    sortable: true 
  },
  { 
    name: "active", 
    label: "Status", 
    field: (r) => r.active !== undefined ? r.active : r.Active, 
    align: "left", 
    sortable: true,
    format: (val) => val ? 'Active' : 'Inactive'
  },
  { 
    name: "createdOn", 
    label: "Created Date", 
    field: (r) => r.createdOn || r.CreatedOn || r.createdOnUtc || r.CreatedOnUtc, 
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

// List Table Composable Integration
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
    pageKey: "billing-methods",
    fetcher: ({ page, limit, sortBy, descending }) => {
        return billingMethodApi.list({
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

const reload = debounce(() => { 
  pagination.value.page = 1; 
  load(); 
}, 300);

watch(search, reload);

onMounted(() => {
  load();
});

const openCreateDrawer = () => {
  editing.value = false;
  editingId.value = null;
  form.name = "";
  form.active = true;
  drawerOpen.value = true;
};

const editRecord = async (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for editing.");
    return;
  }

  editing.value = true;
  editingId.value = id;
  drawerOpen.value = true;
  formLoading.value = true;

  try {
    const response = await billingMethodApi.get(id);
    const item = response?.data?.data || response?.data || response;
    if (item) {
      form.name = item.name || item.Name || "";
      form.active = item.active !== undefined ? item.active : (item.Active !== undefined ? item.Active : true);
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    formLoading.value = false;
  }
};

const closeDrawer = () => {
  drawerOpen.value = false;
};

const submitForm = async () => {
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    const payload = {
      name: form.name,
      active: form.active
    };

    if (editing.value && editingId.value) {
      await billingMethodApi.update(editingId.Value || editingId.value, payload);
      notify.success("Billing method updated successfully.");
    } else {
      await billingMethodApi.create(payload);
      notify.success("Billing method created successfully.");
    }

    drawerOpen.value = false;
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};

/**
 * Triggers the view right-side drawer component for the selected billing method record.
 * @param {Object} row - Target entity row data instance.
 */
const viewRecord = (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) {
    notify.error("Invalid record identifier for viewing.");
    return;
  }
  viewRecordId.value = id;
  viewDrawerOpen.value = true;
};

const deleteRecord = async (row) => {
  const id = row.billingMethodId || row.BillingMethodId || row.id || row.Id;
  if (!id) return;

  const methodLabel = row.name || row.Name || row.billingMethodName || row.BillingMethodName || 'this billing method';
  const ok = await confirm({ 
    title: "Delete Billing Method", 
    message: `Are you sure you want to delete "${methodLabel}"?`, 
    type: "danger" 
  });
  if (!ok) return;

  try {
    await billingMethodApi.delete(id);
    notify.success("Billing method deleted successfully.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const bulkDelete = async (sel) => {
  if (!sel.length) return;
  const ok = await confirm({ 
    title: "Delete Selected", 
    message: `Are you sure you want to delete ${sel.length} billing method(s)?`, 
    type: "danger" 
  });
  if (!ok) return;

  try {
    await Promise.all(sel.map(r => billingMethodApi.delete(r.billingMethodId || r.BillingMethodId || r.id || r.Id)));
    notify.success("Selected billing methods deleted successfully.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>