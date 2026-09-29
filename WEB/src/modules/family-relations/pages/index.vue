<template>
  <q-page padding>
    <!-- List header with breadcrumbs, search, and creation triggers -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Family Relations' }
      ]"
      title="Family Relations"
      description="Manage all family relation types here."
      :search="search"
      show-search
      search-placeholder="Search relations"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Relation"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <!-- Filter drawer for advanced filtering and columns management -->
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

    <!-- Main Data Table displaying relations -->
    <app-data-table
      page-key="family-relations"
      row-key="id"
      title="Relations"
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      :total-records="filteredRows.length"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Status column template -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">
            {{ cell.value ? "Active" : "Inactive" }}
          </q-badge>
        </q-td>
      </template>

      <!-- Row actions column template (View, Edit, Delete) -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn
            flat
            round
            dense
            color="primary"
            icon="o_visibility"
            @click="openView(cell.row)"
          >
            <q-tooltip>View</q-tooltip>
          </q-btn>
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
            @click="removeRelation(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- =========================================================
         Create Family Relation Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="createDrawerOpen"
      title="Create Family Relation"
      :saving="saving"
      save-label="Create"
      @submit="submitCreate"
      @cancel="resetCreateForm"
    >
      <q-form ref="createFormRef" greedy>
        <app-text-field
          v-model="createForm.name"
          label="Relation Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Relation name is required',
            (v) => !v || v.trim().length <= 100 || 'Name cannot exceed 100 characters'
          ]"
        />

        <q-toggle
          v-model="createForm.active"
          label="Active"
        />
      </q-form>
    </app-form-drawer>

    <!-- =========================================================
         Edit Family Relation Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="editDrawerOpen"
      title="Edit Family Relation"
      :saving="saving"
      save-label="Save"
      @submit="submitEdit"
      @cancel="resetEditForm"
    >
      <q-form ref="editFormRef" greedy>
        <app-text-field
          v-model="editForm.name"
          label="Relation Name"
          required
          class="q-mb-md"
          :rules="[
            (v) => !!v?.trim() || 'Relation name is required',
            (v) => !v || v.trim().length <= 100 || 'Name cannot exceed 100 characters'
          ]"
        />

        <q-toggle
          v-model="editForm.active"
          label="Active"
        />
      </q-form>
    </app-form-drawer>

    <!-- =========================================================
         View Family Relation Drawer
         ========================================================= -->
    <app-form-drawer
      v-model="viewDrawerOpen"
      title="View Family Relation"
      :saving="viewLoading"
      :save-label="''"
      :hide-save="true"
      @cancel="closeView"
    >
      <div class="q-gutter-md">
        <div>
          <div class="text-86 fs-12 fw-500">
            Relation Name
          </div>
          <div class="text-2e fs-4">
            {{ viewRelation.name }}
          </div>
        </div>

        <div>
          <div class="text-86 fs-12 fw-500">
            Status
          </div>
          <q-badge :color="viewRelation.active ? 'positive' : 'grey'">
            {{ viewRelation.active ? "Active" : "Inactive" }}
          </q-badge>
        </div>

        <div>
          <div class="text-86 fs-12 fw-500">
            Created By
          </div>
          <div class="text-2e fs-14">
            {{ viewRelation.createdBy || "—" }}
          </div>
        </div>

        <div>
          <div class="text-86 fs-12 fw-500">
            Created On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewRelation.createdOnUtc) }}
          </div>
        </div>

        <div>
          <div class="text-86 fs-12 fw-500">
            Updated By
          </div>
          <div class="text-2e fs-14">
            {{ viewRelation.updatedBy || "—" }}
          </div>
        </div>

        <div>
          <div class="text-86 fs-12 fw-500">
            Updated On
          </div>
          <div class="text-2e fs-14">
            {{ formatDate(viewRelation.updatedOnUtc) }}
          </div>
        </div>
      </div>
    </app-form-drawer>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { debounce } from "quasar";

import {
  familyRelationApi,
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

const canWrite = computed(() => has(Permissions.FamilyRelationsWrite));
const canDelete = computed(() => has(Permissions.FamilyRelationsDelete));

/*
 * ------------------------------------------------------------
 * Table columns configuration including audit details and actions.
 * ------------------------------------------------------------
 */
const columns = [
  {
    name: "name",
    label: "Relation Name",
    field: "name",
    align: "left",
    sortable: true,
    default: true
  },
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
  {
    name: "actions",
    label: "Actions",
    field: "actions",
    align: "left"
  }
];

/*
 * ------------------------------------------------------------
 * List table data fetcher setup with server/client state management.
 * ------------------------------------------------------------
 */
const {
  rows,
  loading,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "family-relations",
  fetcher: ({ sortBy, descending }) =>
    familyRelationApi.list({
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
 * ------------------------------------------------------------
 * View Family Relation Drawer Logic
 * ------------------------------------------------------------
 */
const viewDrawerOpen = ref(false);
const viewLoading = ref(false);

const viewRelation = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const resetViewRelation = () => {
  viewRelation.id = null;
  viewRelation.name = "";
  viewRelation.active = true;
  viewRelation.createdBy = "";
  viewRelation.createdOnUtc = null;
  viewRelation.updatedBy = "";
  viewRelation.updatedOnUtc = null;
};

const openView = async (row) => {
  resetViewRelation();

  viewDrawerOpen.value = true;
  viewLoading.value = true;

  try {
    const relation = await familyRelationApi.get(row.id);

    viewRelation.id = relation?.id;
    viewRelation.name = relation?.name || "";
    viewRelation.active = relation?.active ?? true;
    viewRelation.createdBy = relation?.createdBy || "";
    viewRelation.createdOnUtc = relation?.createdOnUtc || null;
    viewRelation.updatedBy = relation?.updatedBy || "";
    viewRelation.updatedOnUtc = relation?.updatedOnUtc || null;
  } catch (err) {
    viewDrawerOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

const closeView = () => {
  viewDrawerOpen.value = false;
  resetViewRelation();
};

/*
 * Global or local date formatter helper.
 */
const formatDate = (value) => {
  if (!value) {
    return "—";
  }

  return new Date(value).toLocaleString();
};

/*
 * ------------------------------------------------------------
 * Create Family Relation Drawer Logic
 * ------------------------------------------------------------
 */
const createDrawerOpen = ref(false);
const saving = ref(false);
const createFormRef = ref(null);

const createForm = reactive({
  name: "",
  active: true
});

const resetCreateForm = () => {
  createForm.name = "";
  createForm.active = true;
};

const openCreate = () => {
  resetCreateForm();
  createDrawerOpen.value = true;
};

const submitCreate = async ({ clearDraft } = {}) => {
  if (!(await createFormRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: createForm.name.trim(),
      active: createForm.active
    };

    await familyRelationApi.create(payload);
    notify.success("Family relation created successfully.");

    clearDraft?.();
    createDrawerOpen.value = false;
    resetCreateForm();

    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A relation with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

/*
 * ------------------------------------------------------------
 * Edit Family Relation Drawer Logic
 * ------------------------------------------------------------
 */
const editDrawerOpen = ref(false);
const editingId = ref(null);
const editFormRef = ref(null);

const editForm = reactive({
  name: "",
  active: true
});

const resetEditForm = () => {
  editingId.value = null;
  editForm.name = "";
  editForm.active = true;
};

const openEdit = (row) => {
  editingId.value = row.id;
  editForm.name = row.name || "";
  editForm.active = row.active ?? true;

  editDrawerOpen.value = true;
};

const submitEdit = async ({ clearDraft } = {}) => {
  if (!(await editFormRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: editForm.name.trim(),
      active: editForm.active
    };

    await familyRelationApi.update(editingId.value, payload);
    notify.success("Family relation updated successfully.");

    clearDraft?.();
    editDrawerOpen.value = false;
    resetEditForm();

    await load();
  } catch (err) {
    if (getApiErrorCode(err) === ApiErrorCodes.DuplicateIdentifier) {
      notify.error("A relation with this name already exists.");
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};

/*
 * ------------------------------------------------------------
 * Delete Record Handler
 * ------------------------------------------------------------
 */
const removeRelation = async (row) => {
  const ok = await confirm({
    title: "Delete family relation",
    message: `Are you sure you want to delete the relation "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) {
    return;
  }

  try {
    await familyRelationApi.remove(row.id);
    notify.success("Family relation deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>