<template>
  <q-page padding>
    <!-- Page Header Component -->
    <app-list-header
      :breadcrumbs="[
        { label: 'Home', to: '/' },
        { label: 'Policies' }
      ]"
      title="Policies"
      description="Manage your policies and the classes they apply to."
      :search="search"
      show-search
      search-placeholder="Search policy"
      :show-add="canWrite"
      add-label="Add Policy"
      show-back
      @update:search="search = $event"
      @add="openCreate"
      @back="$router.back()"
    />

    <!-- Core Data Table Grid Component -->
    <app-data-table
      page-key="policies"
      row-key="id"
      title="Policies"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <!-- Policy Name Column with Deleted Indicator -->
      <template #body-cell-name="cell">
        <q-td :props="cell" :class="{ 'text-strike text-grey': cell.row.deleted }">
          {{ cell.row.name }}
          <q-badge v-if="cell.row.deleted" color="negative" class="q-ml-sm" dense>
            Deleted
          </q-badge>
        </q-td>
      </template>

      <!-- Classes Column Slot -->
      <template #body-cell-classes="cell">
        <q-td :props="cell">
          <span v-if="!cell.row.classIds?.length">—</span>
          <template v-else>
            {{ classNamesOf(cell.row).slice(0, 2).join(", ") }}
            <q-badge v-if="cell.row.classIds.length > 2" color="grey-6" class="q-ml-xs">
              +{{ cell.row.classIds.length - 2 }}
              <q-tooltip>{{ classNamesOf(cell.row).join(", ") }}</q-tooltip>
            </q-badge>
          </template>
        </q-td>
      </template>

      <!-- Interactive Status Toggle Column Slot -->
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <div class="flex flex-center">
            <q-toggle
              :model-value="cell.row.active"
              dense
              color="positive"
              :disable="cell.row.deleted || !canWrite"
              @update:model-value="(val) => updateStatus(cell.row, val)"
            />
          </div>
        </q-td>
      </template>

      <!-- Row Actions Slot -->
      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn
            v-if="canWrite"
            flat
            round
            dense
            color="primary"
            icon="o_edit"
            :disabled="cell.row.deleted"
            @click="openEdit(cell.row)"
          >
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn
            v-if="canDelete && !cell.row.deleted"
            flat
            round
            dense
            color="negative"
            icon="o_delete"
            @click="removePolicy(cell.row)"
          >
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit Form Dialog Component -->
    <CreateEditPolicy
      v-model="formOpen"
      :editing-id="editingId"
      :class-options="classOptions"
      :classes-loading="classesLoading"
      @saved="handleSaved"
    />

    <!-- View Policy Details Dialog Component -->
    <ViewPolicy
      v-model="viewOpen"
      :view-id="selectedViewId"
      :class-map="classMap"
    />
  </q-page>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";

import { policyApi, classApi, getApiErrorMessage } from "services/api";

import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { Permissions } from "composables/usePermissions";
import { useAuthStore } from "stores/auth";

import AppDataTable from "components/common/AppDataTable.vue";
import AppListHeader from "components/common/AppListHeader.vue";

import CreateEditPolicy from "src/modules/policy/components/create_edit.vue";
import ViewPolicy from "src/modules/policy/components/view.vue";

const { showDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const authStore = useAuthStore();

const canWrite = computed(() => authStore.hasAnyPermission([Permissions.PoliciesWrite]));
const canDelete = computed(() => authStore.hasAnyPermission([Permissions.PoliciesDelete]));

const formOpen = ref(false);
const editingId = ref(null);

const viewOpen = ref(false);
const selectedViewId = ref(null);

// ---- Classes (for the "Applies to Classes" picker and the Classes column) ----
const classes = ref([]);
const classesLoading = ref(false);

const classOptions = computed(() => classes.value.map((c) => ({ label: c.className || "Untitled class", value: c.classId })));
const classMap = computed(() => Object.fromEntries(classOptions.value.map((o) => [o.value, o.label])));
const classNamesOf = (row) => (row.classIds || []).map((id) => classMap.value[id] || "Unknown class");

const loadClasses = async () => {
  classesLoading.value = true;
  try {
    const response = await classApi.list({ limit: 100 });
    classes.value = response?.data || [];
  } catch (err) {
    notify.error("Failed to load classes.");
  } finally {
    classesLoading.value = false;
  }
};

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

const columns = [
  { name: "name", label: "Policy Name", field: "name", align: "left", sortable: true, default: true },
  {
    name: "description",
    label: "Description",
    field: "description",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => (val ? (val.length > 80 ? `${val.slice(0, 80)}…` : val) : "—")
  },
  { name: "classes", label: "Classes", field: (r) => classNamesOf(r).join(", "), align: "left", default: true },
  { name: "displayOrder", label: "Display Order", field: "displayOrder", align: "center", sortable: true, default: false },
  {
    name: "createdOnUtc",
    label: "Created On",
    field: "createdOnUtc",
    align: "left",
    sortable: true,
    default: true,
    format: (val) => formatDate(val)
  },
  { name: "createdBy", label: "Created By", field: (r) => r.createdBy || "—", align: "left", default: true },
  {
    name: "updatedOnUtc",
    label: "Updated On",
    field: "updatedOnUtc",
    align: "left",
    sortable: true,
    default: false,
    format: (val) => formatDate(val)
  },
  { name: "updatedBy", label: "Updated By", field: (r) => r.updatedBy || "—", align: "left", default: false },
  { name: "active", label: "Status", field: "active", align: "center", sortable: true, default: true },
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const {
  rows,
  loading,
  totalRecords,
  search,
  pagination,
  load,
  onRequest
} = useListTable({
  pageKey: "policies",
  defaultSortBy: "createdOnUtc",
  defaultDescending: true,

  fetcher: ({ page, limit, sortBy, descending }) =>
    policyApi.list({
      page,
      limit,
      search: search.value || undefined,
      showDeleted: showDeleted.value,
      sortBy: sortBy || "createdOnUtc",
      descending: descending ?? true
    }).then((response) => ({
      data: response?.data || [],
      total: response?.meta?.totalRecords ?? (response?.data || []).length
    })),

  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => {
  pagination.value.page = 1;
  load();
}, 300);

watch(search, reload);
watch(showDeleted, () => {
  pagination.value.page = 1;
  load();
});

onMounted(() => {
  loadClasses();
  load();
});

const openCreate = () => {
  editingId.value = null;
  formOpen.value = true;
};

const openEdit = (row) => {
  editingId.value = row.id;
  formOpen.value = true;
};

const openView = (row) => {
  selectedViewId.value = row.id;
  viewOpen.value = true;
};

// The toggle sends no classIds, so the API leaves the policy's class mappings as they are. Content is not
// on the list row, so it is read from the full record first rather than being blanked.
const updateStatus = async (row, newStatus) => {
  const originalStatus = row.active;
  row.active = newStatus;

  try {
    const full = await policyApi.get(row.id);
    await policyApi.update(row.id, {
      name: full.name,
      description: full.description,
      content: full.content,
      displayOrder: full.displayOrder,
      active: newStatus
    });
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    row.active = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};

const handleSaved = () => {
  pagination.value.page = 1;
  load();
};

const removePolicy = async (row) => {
  const ok = await confirm({
    title: "Delete policy",
    message: `Are you sure you want to delete the policy "${row.name}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });

  if (!ok) return;

  try {
    await policyApi.delete(row.id);
    notify.success("Policy deleted successfully.");
    await load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
