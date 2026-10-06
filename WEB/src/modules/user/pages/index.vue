<template>
  <q-page padding>
    <app-list-header
      :breadcrumbs="[{ label: 'Home', to: '/' }, { label: 'Staff' }]"
      title="Staffs"
      description="Manage active and inactive staff members, instructors, assignments, schedules, qualifications and staff information."
      :search="search"
      show-search
      search-placeholder="Search name or email"
      show-filters
      :filter-count="filterChips.length"
      :show-add="canCreate"
      add-label="Create Staff"
      show-back
      @update:search="search = $event"
      @filters="filterOpen = true"
      @add="openCreate"
      @back="$router.back()"
    />

    <app-filter-drawer v-model="filterOpen" :chips="filterChips" @remove="removeFilter" @clear="clearFilters">
      <app-column-filters v-model="filters" :columns="filterableColumns" />
      <q-toggle
        v-if="canManageDeleted" v-model="showDeleted" label="Show deleted?" dense class="q-mt-md"
      />
    </app-filter-drawer>

    <app-data-table
      page-key="users"
      row-key="userId"
      title="All Staff"
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
        <q-btn v-if="has(Permissions.UsersWrite)" flat dense no-caps color="positive" label="Activate" @click="bulkSetStatus(sel, true)" />
        <q-btn v-if="has(Permissions.UsersWrite)" flat dense no-caps color="negative" label="Deactivate" @click="bulkSetStatus(sel, false)" />
        <q-btn
          v-if="has(Permissions.UsersResetPassword)" flat dense no-caps color="primary" icon="o_forward_to_inbox"
          label="Send Credentials" :loading="bulkSending" @click="bulkSendCredentials(sel)"
        />
      </template>

      <!-- <template #body-cell-isActive="cell">
        <q-td :props="cell">
          <q-badge :color="cell.value ? 'positive' : 'grey'">{{ cell.value ? "Active" : "Inactive" }}</q-badge>
        </q-td>
      </template> -->

      <template #body-cell-isActive="cell">
        <q-td :props="cell">
          <div class="flex flex-center">
            <q-toggle
              :model-value="cell.row.isActive ?? cell.row.Active ?? cell.row.is_active ?? true"
              @update:model-value="(val) => updateStatus(cell.row, val)"
              dense
              color="positive"
              :disable="!has(Permissions.UsersWrite) || (cell.row.isActive && cell.row.isProtected)">
              <q-tooltip v-if="cell.row.isActive && cell.row.isProtected"> The tenant's default Administrator cannot be deactivated </q-tooltip>
            </q-toggle>
          </div>
        </q-td>
      </template>

      <!-- Department, badged when this user heads it. -->
      <template #body-cell-department="cell">
        <q-td :props="cell">
          <template v-if="cell.value">
            <span>{{ cell.value }}</span>
            <q-icon v-if="cell.row.isDepartmentHead" name="o_workspace_premium" color="primary" size="18px" class="q-ml-xs">
              <q-tooltip>Heads {{ cell.value }}</q-tooltip>
            </q-icon>
          </template>
          <span v-else class="text-grey-6">—</span>
        </q-td>
      </template>

      <!-- <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" :to="{ name: 'user_detail', params: { id: cell.row.userId } }">
            <q-tooltip>View / Manage</q-tooltip>
          </q-btn>
          <-- One button per action, all of them on the row. ->
          <q-btn
            v-if="has(Permissions.UsersWrite)" type="a"
            flat round dense
            :color="cell.row.isActive ? 'grey-8' : 'positive'"
            :icon="cell.row.isActive ? 'o_block' : 'o_check_circle'"
            :disable="cell.row.isActive && cell.row.isProtected"
            @click="setStatus(cell.row, !cell.row.isActive)"
          >
            <q-tooltip>
              {{ cell.row.isActive && cell.row.isProtected
                ? "The tenant's default Administrator cannot be deactivated"
                : (cell.row.isActive ? "Deactivate" : "Activate") }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="has(Permissions.UsersResetPassword)" type="a"
            flat round dense color="primary" icon="o_lock_reset" @click="resetPassword(cell.row)"
          >
            <q-tooltip>Reset Password</q-tooltip>
          </q-btn>
          <q-btn
            v-if="has(Permissions.UsersResetPassword)" type="a"
            flat round dense color="primary" icon="o_forward_to_inbox" @click="sendCredentials(cell.row)"
          >
            <q-tooltip>Send Credentials</q-tooltip>
          </q-btn>
        </q-td>
      </template> -->

      <template #body-cell-actions="cell">
        <q-td :props="cell">
          <q-btn flat round dense color="primary" icon="o_visibility" @click="openView(cell.row.userId)">
            <q-tooltip>View / Manage</q-tooltip>
          </q-btn>
          <q-btn v-if="has(Permissions.UsersWrite)" flat round dense color="primary" icon="o_edit" @click="openEdit(cell.row.userId)">
            <q-tooltip>Edit Staff</q-tooltip></q-btn>
          <q-btn
            v-if="has(Permissions.UsersWrite)" type="a"
            flat round dense
            :color="cell.row.isActive ? 'grey-8' : 'positive'"
            :icon="cell.row.isActive ? 'o_block' : 'o_check_circle'"
            :disable="cell.row.isActive && cell.row.isProtected"
            @click="setStatus(cell.row, !cell.row.isActive)">
            <q-tooltip>
        {{ cell.row.isActive && cell.row.isProtected
          ? "The tenant's default Administrator cannot be deactivated"
          : (cell.row.isActive ? "Deactivate" : "Activate") }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="has(Permissions.UsersResetPassword)" type="a"
            flat round dense color="primary" icon="o_lock_reset" @click="resetPassword(cell.row)"
          >
            <q-tooltip>Reset Password</q-tooltip>
          </q-btn>
          <q-btn
            v-if="has(Permissions.UsersResetPassword)" type="a"
            flat round dense color="primary" icon="o_forward_to_inbox" @click="sendCredentials(cell.row)"
          >
      <q-tooltip>Send Credentials</q-tooltip>
    </q-btn>
  </q-td>
</template>
    </app-data-table>

    <deleted-records-panel
      v-if="canManageDeleted" :entity-type="EntityType.User" :show="showDeleted" @restored="load"
    />

    <!-- Create user (promote an existing Person to a login account). -->
    <user-create-drawer v-model="formOpen" :person-id="presetPersonId" :default-role="STAFF_ROLE" @created="load" />

    <temp-password-dialog v-model="tempPwOpen" :password="tempPassword" />

    <!-- Bulk Send Credentials: the staff whose email could NOT be sent, with their temporary passwords
         so they can be shared manually. Successfully emailed passwords are not shown. -->
    <q-dialog v-model="bulkFailedOpen" persistent>
      <q-card style="min-width: 420px;">
        <q-card-section class="row items-center q-gutter-sm">
          <q-icon name="o_key" color="warning" size="sm" />
          <div class="text-h6">Credentials not emailed</div>
        </q-card-section>
        <q-card-section>
          <div class="text-body2 text-grey-7 q-mb-sm">
            These emails could not be sent. The passwords will not be shown again — share them securely.
          </div>
          <div v-for="f in bulkFailed" :key="f.email" class="q-mb-sm">
            <div class="text-caption text-grey-7">{{ f.name }} — {{ f.email }}</div>
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
  <!-- Staff View Dialog Component -->
    <staff-view-dialog v-model="viewOpen" :user-id="selectedUserId" />

    <user-edit-drawer v-model="editFormOpen" :user-id="selectedEditUserId" @updated="load" />
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { debounce } from "quasar";
import { userApi, getApiErrorMessage, EntityType } from "services/api";
import { usePermissions, Permissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import { useConfirm } from "composables/useConfirm";
import { useListTable } from "composables/useListTable";
import { useColumnFilters } from "composables/useColumnFilters";
import { useDeletedRecords } from "composables/useDeletedRecords";
import { useAuditColumns } from "composables/useAuditColumns";
import { useTenantScope } from "composables/useTenantScope";

import AppDataTable from "components/common/AppDataTable.vue";
import DeletedRecordsPanel from "components/universal/DeletedRecordsPanel.vue";
import AppFilterDrawer from "components/common/AppFilterDrawer.vue";
import AppColumnFilters from "components/common/AppColumnFilters.vue";
import AppListHeader from "components/common/AppListHeader.vue";
import UserCreateDrawer from "components/user/UserCreateDrawer.vue";
import TempPasswordDialog from "components/temp_password_dialog.vue";

import UserEditDrawer from "components/user/UserEditDrawer.vue";

//mport StaffViewDialog from "components/user/pages/detail.vue"; 
import StaffViewDialog from "./detail.vue";
const route = useRoute();
const router = useRouter();

const { showDeleted, canManageDeleted } = useDeletedRecords();
const notify = useNotify();
const { confirm } = useConfirm();
const { has } = usePermissions();
const canCreate = computed(() => has(Permissions.UsersWrite));
const auditColumns = useAuditColumns();

const editFormOpen = ref(false);
const selectedEditUserId = ref(null);

const openEdit = (userId) => {
  selectedEditUserId.value = userId;
  editFormOpen.value = true;
};

// Filterable columns are server-side; text/computed/audit/date columns are covered by the search box.
const columns = computed(() => [
  // No Tenant column: the list is scoped to the active tenant, so every row would repeat the same name.
  // A Super Admin changes which tenant they are looking at with the toolbar's tenant scope.
  { name: "fullName", label: "Name", field: "fullName", align: "left", sortable: true, default: true },
  { name: "email", label: "Email", field: "email", align: "left", sortable: true, default: true },
  { name: "phoneNumber", label: "Phone", field: "phoneNumber", align: "left", sortable: true },
  { name: "roles", label: "Role", field: (r) => (r.roles || []).join(", "), align: "left", sortable: false, default: true, filterable: false },
  { name: "groups", label: "Groups", field: (r) => (r.groups || []).map((g) => g.name).join(", "), align: "left", sortable: false, default: true },
  // Department placement in the active tenant.
  { name: "department", label: "Department", field: "department", align: "left", default: true, filterable: false },
  
  ...auditColumns(),
  { name: "isActive", label: "Status", field: "isActive", align: "center", sortable: true, default: true, filterOptions: [{ label: "Active", value: true }, { label: "Inactive", value: false }] },
  { name: "actions", label: "Actions", field: "actions", align: "left" }
]);

const STAFF_ROLE = "Staff";
// The list only ever shows the tenant selected in the header (Super-Admin scope, else the active tenant).
const { selectedTenantId } = useTenantScope();

const { rows, loading, totalRecords, selected, search, filterOpen, pagination, load, onRequest } = useListTable({
  pageKey: "users",
  fetcher: ({ page, limit, sortBy, descending }) => {
    // No tenant selected (e.g. a Super Admin who has not switched in): nothing to show.
    if (!selectedTenantId.value) return Promise.resolve({ data: [], total: 0 });
    return userApi.list({
      page,
      limit,
      sortBy,
      descending,
      search: search.value || undefined,
      isActive: typeof filters.isActive === "boolean" ? filters.isActive : undefined,
      name: filters.fullName || undefined,
      email: filters.email || undefined,
      phone: filters.phoneNumber || undefined,
      // The Staff list only shows users holding the "Staff" role.
      role: STAFF_ROLE,
      tenantId: selectedTenantId.value,
      group: filters.groups || undefined
    }).then((r) => ({ data: r?.data, total: r?.meta?.totalRecords }));
  },
  onError: (err) => notify.error(getApiErrorMessage(err))
});

// Server-side per-column filters + search box: reload (debounced, first page) whenever they change.
const { filters, filterableColumns, filterChips, removeFilter, clearFilters } = useColumnFilters(columns, rows, { server: true });
const reload = debounce(() => { pagination.value.page = 1; load(); }, 300);
watch([search, filters], reload, { deep: true });

// ---- Create ----
// The drawer owns everything the form needs (people, roles, placements) and loads it when it opens; this
// page only says WHEN to open it and, for the deep-link below, about whom.
const formOpen = ref(false);
const presetPersonId = ref(null);

const openCreate = (personId = null) => {
  presetPersonId.value = personId;
  formOpen.value = true;
};

// "Convert to User" from the People list deep-links here with ?personId=...
onMounted(() => {
  const personId = route.query.personId;
  if (personId && canCreate.value) {
    openCreate(personId);
    // Drop the query param so a refresh doesn't re-open the drawer.
    router.replace({ query: {} });
  }
});

// Shown after an admin password reset. The create drawer has its own for the new account's password.
const tempPwOpen = ref(false);
const tempPassword = ref("");

// ---- Status / reset ----
const setStatus = async (row, isActive) => {
  const ok = await confirm({
    title: isActive ? "Activate user" : "Deactivate user",
    message: `${isActive ? "Activate" : "Deactivate"} ${row.fullName}?`,
    type: isActive ? "primary" : "danger"
  });
  if (!ok) return;
  try {
    await userApi.setStatus(row.userId, isActive);
    notify.success("Status updated.");
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const bulkSetStatus = async (sel, isActive) => {
  if (!sel.length) return;
  // The tenant's default Administrator can't be deactivated (the API rejects it) — skip and warn, same as
  // the People list skips persons already linked to a user on bulk delete.
  const eligible = isActive ? sel : sel.filter((r) => !r.isProtected);
  const skipped = sel.length - eligible.length;
  if (!eligible.length) {
    notify.error("Selected users are protected default Administrators and can't be deactivated.");
    return;
  }
  const ok = await confirm({
    title: isActive ? "Activate users" : "Deactivate users",
    message: `${isActive ? "Activate" : "Deactivate"} ${eligible.length} user(s)?${skipped ? ` (${skipped} protected Administrator(s) will be skipped.)` : ""}`,
    type: isActive ? "primary" : "danger"
  });
  if (!ok) return;
  try {
    await Promise.all(eligible.map((r) => userApi.setStatus(r.userId, isActive)));
    notify.success("Users updated.");
    selected.value = [];
    load();
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

const resetPassword = async (row) => {
  const ok = await confirm({
    title: "Reset password",
    message: `Generate a new temporary password for ${row.fullName}? Their current sessions will end.`,
    confirmLabel: "Reset",
    type: "danger"
  });
  if (!ok) return;
  try {
    const result = await userApi.resetPassword(row.userId);
    tempPassword.value = result?.temporaryPassword || "";
    tempPwOpen.value = true;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};

// Same flow as the Tenants list's "Send Credentials", for one staff user.
const sendCredentials = async (row) => {
  const ok = await confirm({
    title: "Send credentials",
    message: `Email ${row.fullName} their login credentials (username and temporary password)?`,
    confirmLabel: "Send",
    type: "primary"
  });
  if (!ok) return;
  try {
    const result = await userApi.sendCredentials(row.userId);
    // emailSent is the real SMTP outcome (the API sends synchronously for this action).
    const resetText = result?.passwordWasReset ? " A new temporary password was generated (none was on file to resend)." : "";
    if (result?.emailSent) {
      notify.success(`Credentials emailed to ${row.email}.${resetText}`);
    } else {
      notify.warning(`The email could not be sent (check the tenant's SMTP account) — share the credentials below manually.${resetText}`);
    }
    tempPassword.value = result?.temporaryPassword || "";
    tempPwOpen.value = true;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  }
};



// ---- Bulk Send Credentials ----
const bulkSending = ref(false);
const bulkFailedOpen = ref(false);
const bulkFailed = ref([]);

const copyPassword = async (password) => {
  try {
    await navigator.clipboard.writeText(password);
    notify.success("Copied to clipboard.");
  } catch {
    notify.warning("Copy failed — please select and copy manually.");
  }
};

const bulkSendCredentials = async (sel) => {
  if (!sel.length) return;
  const ok = await confirm({
    title: "Send credentials",
    message: `Email ${sel.length} staff member(s) their login credentials (username and temporary password)? ` +
      "Anyone who has already set their own password gets a new temporary password and their sessions end.",
    confirmLabel: "Send",
    type: "primary"
  });
  if (!ok) return;

  bulkSending.value = true;
  let sent = 0;
  const failed = [];   // API succeeded but the email didn't go out — password shown for manual sharing
  const errors = [];   // API call itself failed (permission, not found, ...)
  try {
    // One at a time: each call sends over SMTP synchronously, so this avoids hammering the mail server.
    for (const row of sel) {
      try {
        const result = await userApi.sendCredentials(row.userId);
        if (result?.emailSent) {
          sent++;
        } else {
          failed.push({ name: row.fullName, email: row.email, password: result?.temporaryPassword || "" });
        }
      } catch (err) {
        errors.push(`${row.fullName}: ${getApiErrorMessage(err)}`);
      }
    }
  } finally {
    bulkSending.value = false;
  }

  if (sent) notify.success(`Credentials emailed to ${sent} staff member(s).`);
  if (errors.length) notify.error(`Could not send credentials — ${errors.join("; ")}`);
  if (failed.length) {
    notify.warning(`${failed.length} email(s) could not be sent (check the tenant's SMTP account).`);
    bulkFailed.value = failed;
    bulkFailedOpen.value = true;
  }
  selected.value = [];
};

// ---- View Dialog State ----
const viewOpen = ref(false);
const selectedUserId = ref(null);

const openView = (userId) => {
  selectedUserId.value = userId;
  viewOpen.value = true;
};



// ---- Update Status toggle in the table ----
const updateStatus = async (row, newStatus) => {
  const id = row.userId;
  if (!id) return;

  const originalStatus = row.isActive;
  row.isActive = newStatus;

  try {
    await userApi.setStatus(id, newStatus);
    notify.success("Status updated successfully.");
    await load();
  } catch (err) {
    row.isActive = originalStatus;
    notify.error(getApiErrorMessage(err));
  }
};

</script>
