<template>
  <app-form-dialog
    v-model="isOpen"
    :title="role ? role.name : 'Role Details'"
    :subtitle="roleSubtitle"
    avatar-text="R"
    size="md"
    hide-save
    hide-footer
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else-if="role" class="view-student-scss">
      <!-- =====================================================
           SUMMARY CARDS
           ===================================================== -->
      <div class="student-summary row q-col-gutter-md q-mb-md">
        <div class="col-12 col-sm-12">
          <div class="summary-card">
            <div class="summary-card__label">PERMISSIONS COUNT</div>
            <div class="summary-card__value font-mono">
              {{ computedPermissionCount }}
            </div>
          </div>
        </div>
      </div>

      <!-- =====================================================
           ROLE DETAILS + ADDITIONAL INFO
           ===================================================== -->
      <div class="row q-col-gutter-md">
        <!-- Role Information -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_admin_panel_settings" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">ROLE INFORMATION</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Name</span>
                <span class="fs-11 fw-600 text-2e">{{ role.name || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Display Name</span>
                <span class="fs-11 fw-600 text-2e">{{ role.displayName || "—" }}</span>
              </div>
            </div>
          </section>
        </div>

        <!-- Additional Info / Description -->
        <div class="col-12 col-md-6">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_info" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">DESCRIPTION & TENANT</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Tenant Name</span>
                <span class="fs-11 fw-600 text-2e">{{ computedTenantName }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Description</span>
                <span class="fs-11 fw-600 text-2e" v-html="role.description || '—'"></span>
              </div>
            </div>
          </section>
        </div>
      </div>

      <!-- =====================================================
           AUDIT
           ===================================================== -->
      <div class="mt-24">
        <app-record-audit :audit="role.audit" />
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { roleApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppRecordAudit from "components/common/AppRecordAudit.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  roleId: { type: String, default: null }
});
const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const role = ref(null);
const loading = ref(false);

const isOpen = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v)
});

const roleSubtitle = computed(() => {
  return `Role Details & Permissions`;
});

// Computed properties for safe data mapping
const computedPermissionCount = computed(() => {
  if (!role.value) return 0;
  if (Array.isArray(role.value.permissions)) {
    return role.value.permissions.length;
  }
  return role.value.permissionCount || 0;
});

const computedTenantName = computed(() => {
  if (!role.value) return "Platform";
  return role.value.tenantName || role.value.TenantName || role.value.tenant?.name || (role.value.tenantId ? "Assigned Tenant" : "Platform");
});

const load = async () => {
  if (!props.roleId) return;
  loading.value = true;
  try {
    role.value = await roleApi.get(props.roleId);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};

watch(
  () => props.modelValue,
  (val) => {
    if (val) load();
  }
);

watch(
  () => props.roleId,
  () => {
    if (props.modelValue) load();
  }
);
</script>