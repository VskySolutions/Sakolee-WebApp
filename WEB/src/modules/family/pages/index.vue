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
      selectable
      @request="onRequest"
      @refresh="load"
      @update:selected="selected = $event"
    >
      <template #bulk-actions="{ selected: sel }">
        <q-btn
          v-if="canWrite" flat dense no-caps color="primary" icon="o_forward_to_inbox"
          label="Send Credentials" :loading="sendingCredentials" @click="sendCredentials(sel)"
        />
      </template>

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
          <q-btn v-if="canWrite" flat round dense color="primary" icon="o_forward_to_inbox" @click="sendCredentials([cell.row])">
            <q-tooltip>Send Credentials to Parents</q-tooltip>
          </q-btn>
          <q-btn v-if="canDelete" flat round dense color="negative" icon="o_delete" @click="remove(cell.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </app-data-table>

    <!-- Create / Edit drawer -->
    <family-form-drawer
      v-model="formOpen"
      :mode="formMode"
      :family-id="formFamilyId"
      :family-status-options="familyStatusOptions"
      @saved="load"
    />

    <!-- Read-only View drawer -->
    <family-view-drawer
      v-model="viewOpen"
      :family-id="viewFamilyId"
      :family-status-options="familyStatusOptions"
    />

    <!-- Send Credentials: parents whose email could NOT be sent, with their temporary passwords so they
         can be shared manually. Successfully emailed passwords are not shown. -->
    <q-dialog v-model="credentialsFailedOpen" persistent>
      <q-card style="min-width: 420px;">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_key" color="warning" size="sm" />
          <div class="text-h6">Credentials not emailed</div>
        </q-card-section>
        <q-card-section>
          <div class="text-body2 text-grey-7 q-mb-sm">
            These emails could not be sent. The passwords will not be shown again — share them securely.
          </div>
          <div v-for="f in credentialsFailed" :key="f.email" class="q-mb-sm">
            <div class="text-caption text-grey-7">{{ f.familyName }} · {{ f.name }} — {{ f.email }}</div>
            <q-input :model-value="f.password" readonly outlined dense>
              <template #append>
                <q-btn flat round dense icon="o_content_copy" @click="copyPassword(f.password)">
                  <q-tooltip>Copy</q-tooltip>
                </q-btn>
              </template>
            </q-input>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn v-close-popup flat no-caps color="primary" label="Done" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
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
import FamilyViewDrawer from "modules/family/components/FamilyViewDrawer.vue";

const router = useRouter();
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
    // Only active statuses are offered for selection (inactive ones stay on the Family Status page).
    const statusRes = await familyStatusApi.list({ limit: 100, active: true });
    familyStatuses.value = statusRes?.data || [];
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
});

const filters = reactive({ familyStatusId: null });
const { rows, loading, totalRecords, selected, search, filterOpen, pagination, load, onRequest } = useListTable({
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

// ---- Create / Edit (FamilyFormDrawer) ----
const formOpen = ref(false);
const formMode = ref("create");
const formFamilyId = ref(null);

const openForm = (mode, familyId = null) => {
  formMode.value = mode;
  formFamilyId.value = familyId;
  formOpen.value = true;
};
// Creating a family goes through the full Quick Registration wizard (family + contacts + students).
const openCreate = () => router.push({ name: "family_quick_registration" });
const openEdit = (row) => openForm("edit", row.familyId);

// ---- Read-only View (FamilyViewDrawer) ----
const viewOpen = ref(false);
const viewFamilyId = ref(null);
const openView = (row) => {
  viewFamilyId.value = row.familyId;
  viewOpen.value = true;
};

// ---- Send Credentials (parents only — the API never includes students) ----
const sendingCredentials = ref(false);
const credentialsFailedOpen = ref(false);
const credentialsFailed = ref([]);

const copyPassword = async (password) => {
  try {
    await navigator.clipboard.writeText(password);
    notify.success("Copied to clipboard.");
  } catch {
    notify.warning("Copy failed — please select and copy manually.");
  }
};

// Used by both the row button (one family) and the bulk action (selected families).
const sendCredentials = async (families) => {
  if (!families.length) return;
  const ok = await confirm({
    title: "Send credentials",
    message: `Email the parents of ${families.length === 1 ? `"${families[0].familyName}"` : `${families.length} families`} ` +
      "their login credentials (username and temporary password)? Anyone who has already set their own " +
      "password gets a new temporary password and their sessions end.",
    confirmLabel: "Send",
    type: "primary"
  });
  if (!ok) return;

  sendingCredentials.value = true;
  let sent = 0;
  const failed = [];   // email didn't go out — password shown for manual sharing
  const errors = [];   // API call failed for the whole family (no parent login, permission, ...)
  try {
    // One family at a time: each call sends over SMTP synchronously.
    for (const family of families) {
      try {
        const result = await familyApi.sendCredentials(family.familyId);
        for (const c of result?.contacts || []) {
          if (c.emailSent) {
            sent++;
          } else {
            failed.push({ familyName: family.familyName, name: c.name, email: c.email, password: c.temporaryPassword });
          }
        }
      } catch (err) {
        errors.push(`${family.familyName}: ${getApiErrorMessage(err)}`);
      }
    }
  } finally {
    sendingCredentials.value = false;
  }

  if (sent) notify.success(`Credentials emailed to ${sent} parent(s).`);
  if (errors.length) notify.error(`Could not send credentials — ${errors.join("; ")}`);
  if (failed.length) {
    notify.warning(`${failed.length} email(s) could not be sent (check the tenant's SMTP account).`);
    credentialsFailed.value = failed;
    credentialsFailedOpen.value = true;
  }
  selected.value = [];
};

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
