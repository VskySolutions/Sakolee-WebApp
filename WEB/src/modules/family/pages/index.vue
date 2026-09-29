<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'All Families' }]"
      title="All Families"
      description="Manage student families and parent records."
      :search="search"
      show-search
      search-placeholder="Search family name"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canWrite"
      add-label="Create Family"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-select v-model="filters.familyStatusId" label="Family Status" :options="familyStatusOptions" />
    </app-filter-drawer>

    <app-data-table
      page-key="families"
      row-key="familyId"
      title="All Families"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :total-records="totalRecords"
      :pagination="pagination"
      @request="onRequest"
      @refresh="load"
    >
      <template #body-cell-active="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template>

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row)">
            <q-tooltip>View</q-tooltip>
          </q-btn>
          <q-btn v-if="canWrite" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row)">
            <q-tooltip>Edit</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit / View drawer -->
    <family-form-drawer
      v-model="formOpen"
      :mode="formMode"
      :family-id="formFamilyId"
      :family-status-options="familyStatusOptions"
      @saved="load"
    />
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { debounce } from "quasar";
import { familyApi, familyStatusApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useAuditColumns } from "composables/useAuditColumns";
import { usePermissions, Permissions } from "composables/usePermissions";

import AppDataTable from "components/common/AppDataTable.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import AppSelect from "components/common/AppSelect.vue";
import FamilyFormDrawer from "modules/family/components/FamilyFormDrawer.vue";

const notify = useNotify();
const { confirm } = useConfirm();
const auditColumns = useAuditColumns();
const { has } = usePermissions();
const canWrite = computed(() => has(Permissions.FamiliesWrite));
const canDelete = computed(() => has(Permissions.FamiliesDelete));

const columns = [
  { name: "familyName", label: "Family Name", field: "familyName", align: "left", default: true, sortable: true },
  { name: "primaryContactName", label: "Primary Contact", field: (row) => row.primaryContactName || "—", align: "left", default: true },
  { name: "primaryContactEmail", label: "Email", field: (row) => row.primaryContactEmail || "—", align: "left", default: true },
  { name: "primaryContactPhone", label: "Phone", field: (row) => row.primaryContactPhone || "—", align: "left" },
  { name: "familyStatusName", label: "Status", field: (row) => row.familyStatusName || "—", align: "left" },
  { name: "studioLocationName", label: "Studio Location", field: (row) => row.studioLocationName || "—", align: "left" },
  { name: "studentCount", label: "Students", field: "studentCount", align: "left" },
  { name: "active", label: "Active", field: "active", align: "left", default: true },
  ...auditColumns(),
  { name: "actions", label: "Actions", field: "actions", align: "left" }
];

// ---- Reference option lists ----
const familyStatuses = ref([]);
// FamilyStatusSummary's id field is just "id" (Guid Id), not "familyStatusId" — mirrors the
// familystatus module's own pages, which fall back through the same mismatch.
const familyStatusOptions = computed(() => familyStatuses.value.map((s) => ({ label: s.name, value: s.id })));

onMounted(async () => {
  try {
    const statusRes = await familyStatusApi.list({ limit: 100 });
    familyStatuses.value = statusRes?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

const filters = reactive({ familyStatusId: null });
const { rows, loading, totalRecords, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "families",
  fetcher: ({ page, limit, sortBy, descending }) =>
    familyApi.list({
      page,
      limit,
      sortBy,
      descending,
      familyStatusId: filters.familyStatusId || undefined,
      search: search.value || undefined
    })
      .then((r) => ({ data: r?.data, total: r?.meta?.totalRecords })),
  onError: (err) => notify.error(getApiErrorMessage(err))
});

const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

const filterChips = computed(() => {
  const chips = [];
  if (filters.familyStatusId) {
    const found = familyStatusOptions.value.find((o) => o.value === filters.familyStatusId);
    chips.push({ key: "familyStatusId", label: `Status: ${found ? found.label : filters.familyStatusId}` });
  }
  return chips;
});
const removeFilter = (key) => { if (key === "familyStatusId") filters.familyStatusId = null; };
const clearFilters = () => { filters.familyStatusId = null; };

// ---- Create / Edit / View (form lives in FamilyFormDrawer) ----
const formOpen = ref(false);
const formMode = ref("create");
const formFamilyId = ref(null);

const openForm = (mode, familyId = null) => {
  formMode.value = mode;
  formFamilyId.value = familyId;
  formOpen.value = true;
};
const openCreate = () => openForm("create");
const openEdit = (row) => openForm("edit", row.familyId);
const openView = (row) => openForm("view", row.familyId);

const remove = async (row) => {
  const ok = await confirm({
    title: "Delete family",
    message: `Delete "${row.familyName}"?`,
    confirmLabel: "Delete",
    type: "danger"
  });
  if (!ok) return;
  try {
    await familyApi.remove(row.familyId);
    notify.success("Family deleted.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};
</script>
