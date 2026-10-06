<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Classes' }]"
      title="All Classes"
      description="Manage all classes."
      :search="search"
      show-search
      search-placeholder="Search class name"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Add Class"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.active" :options="activeFilterOptions" label="Status" />
    </app-filter-drawer>

    <app-data-table
      page-key="classes"
      row-key="classId"
      title="All Classes"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      selectable
      @request="onRequest"
      @refresh="load"
    >
    <template v-if="canDelete" #bulk-actions="{ selected: sel }">
    <q-btn flat dense no-caps color="negative" label="Delete" @click="bulkDelete(sel)" />
  </template>
      <!-- <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template> -->

      <template #body-cell-active="cell">
  <q-td :props="cell">
    <div class="flex flex-center">
      <q-toggle
        :model-value="cell.row.active ?? cell.row.Active ?? cell.row.isActive ?? cell.row.IsActive ?? true"
        @update:model-value="(val) => updateStatus(cell.row, val)"
        dense
        color="positive"
        :disable="!canWrite"
      />
    </div>
  </q-td>
</template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn v-if="canWrite" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row.classId)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>
    <create-edit v-model="formOpen" :class-id="editingId" @saved="load" />
    <view-class v-model="viewOpen" :record-id="viewRecordId" />
  </q-page>
</template>

<script setup>
import { ref, computed, reactive, watch } from "vue";
import { debounce } from "quasar";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { useDateFormat } from "composables/useDateFormat";
import { usePermissions, Permissions } from "composables/usePermissions";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import CreateEdit from "modules/class/components/CreateEdit.vue";
import ViewClass from "modules/class/components/view_class.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { formatDate } = useDateFormat();
const { has } = usePermissions();
const canWrite = computed(() => has(Permissions.ClassesWrite));
const canDelete = computed(() => has(Permissions.ClassesDelete));

const columns = [
  { name: "className", label: "Class Name", field: "className", align: "left", sortable: true, default: true },
  { name: "startDate", label: "Start Date", field: (row) => formatDate(row.startDate), sort: (row) => row.startDate || "", align: "left", sortable: true, default: true },
  { name: "endDate", label: "End Date", field: (row) => formatDate(row.endDate), sort: (row) => row.endDate || "", align: "left", sortable: true },
  { name: "startTime", label: "Start Time", field: (row) => row.startTime || "—", align: "left" },
  { name: "endTime", label: "End Time", field: (row) => row.endTime || "—", align: "left" },
  { name: "tuitionFee", label: "Tuition Fee", field: (row) => row.tuitionFee ?? "—", align: "left" },
  { name: "maxClassSize", label: "Max Size", field: (row) => row.maxClassSize ?? "—", align: "left" },
  
  ...auditColumns(),
  { name: "active", label: "Status", field: "active", align: "center", sortable: true, default: true },
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

const filters = reactive({ active: null });
const { rows, loading, totalRecords,selected, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "classes",
  fetcher: ({ page, limit, sortBy, descending }) =>
    classApi.list({
      page,
      limit,
      sortBy,
      descending,
      active: filters.active === null ? undefined : filters.active,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const activeFilterOptions = [{ label: "Active", value: true }, { label: "Inactive", value: false }];

const filterChips = computed(() => {
  const chips = [];
  if (filters.active !== null) chips.push({ key: "active", label: `Status: ${filters.active ? "Active" : "Inactive"}` });
  return chips;
});

const removeFilter = (key) => {
  if (key === "active") filters.active = null;
};
const clearFilters = () => {
  filters.active = null;
};

const remove = async (row) => {
  const ok = await confirm({
    title: "Delete class",
    message: `Delete "${row.className}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await classApi.remove(row.classId);
    notify.success("Class deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

/*
 * ------------------------------------------------------------
 * Add / Edit Class (popup)
 * ------------------------------------------------------------
 */
const formOpen = ref(false);
const editingId = ref(null);
const openCreate = () => {
  editingId.value = null;
  formOpen.value = true;
};
const openEdit = (classId) => {
  editingId.value = classId;
  formOpen.value = true;
};

const viewOpen = ref(false);
const viewRecordId = ref(null);

const openView = (row) => {
  viewRecordId.value = row.classId;
  viewOpen.value = true;
};

const updateStatus = async (row, newStatus) => {
  const id = row.classId;
  if (!id) return;

  const originalStatus = row.active;
  row.active = newStatus;

  try {
    await classApi.update(id, { ...row, active: newStatus });
    notify.success("Class active status updated successfully.");
    await load();
  } catch (err) {
    row.active = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};
const bulkDelete = async (sel) => {
  if (!sel.length) return;
  const ok = await confirm({
    title: "Delete classes",
    message: `Delete ${sel.length} class(es)? This cannot be undone.`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await Promise.all(sel.map((r) => classApi.remove(r.classId)));
    notify.success("Classes deleted successfully.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
