<template>
  <q-page padding>
    <!-- Page header with breadcrumbs, search, and create button -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'T-Shirt Sizes' }
      ]"
      :search="search"
      show-search
      search-placeholder="Search T-Shirt size name"
      :show-add="canWrite"
      add-label="Create T-Shirt Size"
      show-back
      @update:search="search = $event"
      @add="openCreate"
      @back="$router.back()"
    />
    <!-- Table displaying all T-Shirt Sizes -->
    <app-data-table
      page-key="t-shirt-sizes"
      row-key="tShirtSizeId"
      title="T-Shirt Sizes"
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
        <q-td :props="cell" text-class="text-center" style="padding-right: 150px !important;">
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
            @click="deleteTShirtSize(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <!-- Drawer used for creating and editing T-Shirt Sizes -->
    <create-edit
      v-model="formOpen"
      :editing="editing"
      :t-shirt-size="selectedTShirtSize"
      @saved="load"
    />
    <!-- Drawer used to display T-Shirt Size details -->
    <app-form-drawer
      v-model="viewOpen"
      title="View T-Shirt Size"
      :saving="viewLoading"
      :save-label="''"
      @cancel="closeView"
    >
      <div class="q-gutter-md">
        <app-text-field
          v-model="viewTShirtSize.name"
          label="Name"
          readonly
        />
        <app-text-field
          v-model="viewTShirtSize.tenantName"
          label="Tenant"
          readonly
        />
        <q-separator />
        <div>
          <div class="text-caption text-grey-7">
            Created On
          </div>
          <div class="text-body1">
            {{ formatDateTime(viewTShirtSize.createdOnUtc) }}
          </div>
        </div>
      </div>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { computed, ref, watch } from "vue";
import { date, debounce } from "quasar";
import { tShirtSizeApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { usePermissions } from "composables/usePermissions";
import { useListTable } from "composables/useListTable";
import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue"; 
import AppTextField from "components/common/AppTextField.vue";
import CreateEdit from "modules/t-shirt-size/components/CreateEdit.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
// Check permissions for T-Shirt Size actions.
const canRead = computed(() => has("tShirtSizes.read"));
const canWrite = computed(() => has("tShirtSizes.write"));
const canDelete = computed(() => has("tShirtSizes.delete"));
// Format UTC date values for display.
const formatDateTime = (value) => { if (!value) { return "—"; }
  // Add UTC indicator when the API date does not include timezone information.
  const iso =/(Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : `${value}Z`;
  return date.formatDate(new Date(iso),"MM/DD/YYYY hh:mm A");
};
// Define the columns displayed in the T-Shirt Size table.
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
    align: "right",
    style: "padding-right: 150px !important;",
    headerStyle: "padding-right: 190px !important;"
  }
];
// Configure the reusable list table functionality.
const { rows ,loading ,totalRecords ,search ,pagination ,load ,onRequest } = useListTable({
  pageKey: "t-shirt-sizes",
  // Fetch T-Shirt Sizes from the API.
  fetcher: ({ page, limit, sortBy, descending }) =>
    tShirtSizeApi.list({ search: search.value || undefined })
      .then((response) => { const data = Array.isArray(response?.data) ? response.data : [];
        return { data, total: data.length };
      }),
  onError: (error) => { notify.error( getApiErrorMessage( error, "Unable to load T-Shirt sizes."));}
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
const selectedTShirtSize = ref(null);
const viewOpen = ref(false);
const viewLoading = ref(false);
// Define the structure of the T-Shirt Size being viewed.
const viewTShirtSize = ref({
  tShirtSizeId: null,
  name: "",
  tenantName: "",
  createdOnUtc: null
});
// Reset the viewTShirtSize to its initial state.
const resetViewTShirtSize = () => {
  viewTShirtSize.value = {
    tShirtSizeId: null,
    name: "",
    tenantName: "",
    createdOnUtc: null
  };
};
// Open the View drawer for the selected T-Shirt Size.
const openView = async (row) => {
  resetViewTShirtSize();
  viewOpen.value = true;
  viewLoading.value = true;
  try {
    const tShirtSize = await tShirtSizeApi.get(
      row.tShirtSizeId
    );
    viewTShirtSize.value = {
      tShirtSizeId: tShirtSize?.tShirtSizeId,
      name: tShirtSize?.name || "",
      tenantName: tShirtSize?.tenantName || "",
      createdOnUtc: tShirtSize?.createdOnUtc || null
    };
  } catch (error) {
    viewOpen.value = false;
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load T-Shirt size."
      )
    );
  } finally {
    viewLoading.value = false;
  }
};
// Close the View drawer and reset the viewTShirtSize state.
const closeView = () => {
  viewOpen.value = false;
  resetViewTShirtSize();
};

// Open the form for creating a new T-Shirt Size.
const openCreate = () => {
  selectedTShirtSize.value = null;
  editing.value = false;
  formOpen.value = true;
};

// Load the selected T-Shirt Size and open the edit form.
const openEdit = async (row) => {
  try {
    // Get the latest T-Shirt Size details from the API.
    const tShirtSize = await tShirtSizeApi.get(row.tShirtSizeId);
    selectedTShirtSize.value = tShirtSize;
    editing.value = true;
    formOpen.value = true;
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to load T-Shirt size."
      )
    );
  }
};

// Delete the selected T-Shirt Size.
const deleteTShirtSize = async (row) => {
  const confirmed = await confirm({
    title: "Delete T-Shirt Size",
    message: `Delete "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  // Stop if the user cancels the operation.
  if (!confirmed) {
    return;
  }
  try {
    await tShirtSizeApi.remove(
      row.tShirtSizeId
    );
    notify.success("T-Shirt size deleted.");
    load();
  } catch (error) {
    notify.error(
      getApiErrorMessage(
        error,
        "Unable to delete T-Shirt size."
      )
    );
  }
};
// Load T-Shirt Sizes when the page is opened.
load();
</script>