<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Locations' }
      ]"
      :search="search"
      show-search
      search-placeholder="Search locations"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Location"
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
      page-key="locations"
      row-key="id"
      title="Locations"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Status -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">
            {{ cell.value ? "Active" : "Inactive" }}
          </q-badge>
        </q-td>
      </template>

      <!-- Actions -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn
            v-if="canWrite"
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>

          <q-btn
            v-if="canDelete"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removeLocation(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- <deleted-records-panel
      v-if="canManageDeleted"
      :entity-type="EntityType.Location"
      :show="showDeleted"
      @restored="load"
    /> -->

    <!-- Create / Edit -->
    <app-form-drawer
      v-model="formOpen"
      :title="editingId ? 'Edit Location' : 'Create Location'"
      :saving="saving"
      :save-label="editingId ? 'Save' : 'Create'"
      @submit="submitForm"
      @cancel="resetForm"
    >
      <q-form ref="formRef" greedy>
        <app-text-field
          v-model="form.name"
          label="Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Location name is required',
            (v) =>
              !v ||
              v.trim().length <= 100 ||
              'Location name cannot exceed 100 characters'
          ]"
        />

        <q-toggle
          v-model="form.active"
          label="Active"
        />
      </q-form>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";

import {
  locationApi,
  getApiErrorMessage,
  getApiErrorCode,
  ApiErrorCodes
} from "services/api";

import { usePermissions, Permissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useAuditColumns } from "composables/useAuditColumns";

import AppDataTable from "components/common/AppDataTable.vue";
// import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppFormDrawer from "components/common/AppFormDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";

import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppTextField from "components/common/AppTextField.vue";

const auditColumns = useAuditColumns();
const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();

const canWrite = computed(() => has(Permissions.LocationsWrite));
const canDelete = computed(() => has(Permissions.LocationsDelete));

const columns = [
  { name: "name", label: "Name", field: "name", align: "left", sortable: true, default: true },
  {
    name: "active",
    label: "Status",
    field: "active",
    align: "left",
    sortable: true,
    default: true,
    filterOptions: [
      { label: "Active", value: true },
      { label: "Inactive", value: false }
    ]
  },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "locations",

  fetcher: ({ sortBy, descending }) =>
    locationApi.list({
      search: search.value || undefined,
      sortBy,
      descending
    }).then((response) => ({
      data: response?.data || [],
      total: (response?.data || []).length
    })),

  onError: (err) => notify.error(getApiErrorMessage(err))
});

const filterOpen = ref(false);

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

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);

/*
 * Create / Edit
 */
const formOpen = ref(false);
const saving = ref(false);
const editingId = ref(null);
const formRef = ref(null);

const form = reactive({
  name: "",
  active: true
});

const resetForm = () => {
  editingId.value = null;
  form.name = "";
  form.active = true;
};

const openCreate = () => {
  resetForm();
  formOpen.value = true;
};

const openEdit = (row) => {
  editingId.value = row.id;
  form.name = row.name || "";
  form.active = row.active ?? true;
  formOpen.value = true;
};

const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim(),
      active: form.active
    };

    if (editingId.value) {
      await locationApi.update(editingId.value, payload);
      notify.success("Location updated.");
    } else {
      await locationApi.create(payload);
      notify.success("Location created.");
    }

    clearDraft?.();

    formOpen.value = false;
    resetForm();

    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A location with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

/*
 * Delete
 */
const removeLocation = async (row) => {
  const ok = await confirm({
    title: "Delete location",
    message: `Delete the "${row.name}" location?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) {
    return;
  }

  try {
    await locationApi.remove(row.id);

    notify.success("Location deleted.");

    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
