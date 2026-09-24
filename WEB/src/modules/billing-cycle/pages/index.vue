<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Billing Cycles' }
      ]"
      :search="search"
      show-search
      search-placeholder="Search billing cycle name"
      :show-add="canWrite"
      add-label="Create Billing Cycle"
      show-back
      @update:search="search = $event"
      @add="openCreate"
      @back="$router.back()"
    />
    <!-- Table displaying all Billing Cycles -->
    <app-data-table
      page-key="billing-cycles"
      row-key="billingCycleId"
      title="All Billing Cycles"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Action buttons for viewing, editing, and deleting -->
      <template #body-cell-actions="cell">
        <q-td :props="cell" text-class="text-center">
          <!-- View button -->
          <q-btn
            v-if="canRead"
            type="a"
            flat
            round
            dense
            color="primary"
            icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <!-- Edit button -->
          <q-btn
            v-if="canWrite"
            type="a"
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <!-- Delete button -->
          <q-btn
            v-if="canDelete"
            type="a"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="deleteBillingCycle(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Drawer used for creating and editing Billing Cycles -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :billing-cycle="selectedBillingCycle"
      @saved="load"
    />
    <!-- Dialog used to display Billing Cycle details -->
    <q-dialog
      v-model="viewOpen"
      position="right"
      transition-show="slide-left"
      transition-hide="slide-right"
    >
      <q-card
        class="column no-wrap"
        style="
          width: 350px;
          max-width: 100vw;
          max-height: 70vh;
        "
      >
        <!-- Dialog header -->
        <q-card-section class="row items-center">
          <div class="text-h6 text-primary">
            Billing Cycle Details
          </div>
          <q-space />
          <!-- Close details dialog -->
          <q-btn
            icon="o_close"
            flat
            round
            dense
            v-close-popup
          />
        </q-card-section>
        <q-separator />
        <!-- Display selected Billing Cycle details -->
        <q-card-section class="col scroll">
          <q-list>
            <q-item
              v-for="field in viewFields"
              :key="field.label"
            >
              <q-item-section>
                <q-item-label caption>
                  {{ field.label }}
                </q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-item-label v-if="field.badge">
                  <q-badge color="primary">
                    {{ field.value }}
                  </q-badge>
                </q-item-label>
                <!-- Display normal text value -->
                <q-item-label v-else>
                  {{ field.value }}
                </q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { billingCycleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import CreateEdit from "modules/billing-cycle/components/CreateEdit.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
// Check permissions for Billing Cycle actions.
const canRead = computed(() =>
  has("billingCycles.read")
);
const canWrite = computed(() =>
  has("billingCycles.write")
);
const canDelete = computed(() =>
  has("billingCycles.delete")
);
// Format UTC date values for display.
const formatDateTime = (value) => {
  if (!value) {
    return "—";
  }
 // Add UTC indicator when the API date does not include timezone information.
  const iso =/(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso), "MM/DD/YYYY hh:mm A");
};

// Define the columns displayed in the Billing Cycle table.
const columns = [
  {
    name: "name",
    label: "Name",
    field: "name",
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "tenantName",
    label: "Tenant",
    field: "tenantName",
    align: "left",
    sortable: true,
    default: true
  },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: "createdOnUtc",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => formatDateTime(val)
  },
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

// Configure the reusable list table functionality.
const { rows, loading, totalRecords, search, pagination, load, onRequest } = useListTable({
  pageKey: "billing-cycles",
  // Fetch Billing Cycles from the API.
  fetcher: ({ page, limit, sortBy, descending }) => billingCycleApi .list({ search: search.value || undefined })
      .then((response) => {
        const data = Array.isArray(response?.data)
          ? response.data : [];
        return { data, total: data.length };
      }),
  onError: (error) => { notify.error( getApiErrorMessage( error, "Unable to load billing cycles.") ); }
});

// Reload the table when the search value changes.
// Debounce prevents an API call for every keystroke.
const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);
const formOpen = ref(false);
const editing = ref(false);
const selectedBillingCycle = ref(null);
const viewOpen = ref(false);
const viewed = ref(null);

// Prepare fields to display in the view dialog.
const viewFields = computed(() =>
  viewed.value
    ? [
        {
          label: "Name",
          value: viewed.value.name || "—"
        },
        {
          label: "Tenant",
          value: viewed.value.tenantName || "—"
        },
        {
          label: "Created On",
          value: formatDateTime(
            viewed.value.createdOnUtc
          )
        }
      ]
    : []
);

// Open the view dialog for the selected Billing Cycle.
const openView = (row) => {
  viewed.value = row;
  viewOpen.value = true;
};

// Open the form for creating a new Billing Cycle.
const openCreate = () => {
  selectedBillingCycle.value = null;
  editing.value = false;
  formOpen.value = true;
};

// Load the selected Billing Cycle and open the edit form.
const openEdit = async (row) => {
  try {
  // Get the latest Billing Cycle details from the API.
    const billingCycle = await billingCycleApi.get(
      row.billingCycleId
    );
    selectedBillingCycle.value = billingCycle;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load billing cycle."
      )
    );
  }
};

// Delete the selected Billing Cycle.
const deleteBillingCycle = async (row) => {
  const confirmed = await confirm({
    title: "Delete Billing Cycle",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
// Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    await billingCycleApi.remove(
      row.billingCycleId
    );
    notify.success("Billing cycle deleted.");
    load();
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to delete billing cycle."
      )
    );
  }
};
// Load Billing Cycles when the page is opened.
load();
</script>
