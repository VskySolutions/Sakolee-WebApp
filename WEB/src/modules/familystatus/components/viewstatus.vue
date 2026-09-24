<template>
  <!-- 
    ============================================================
    View Family Status Drawer Component
    ============================================================
    Renders a read-only side drawer displaying comprehensive details 
    and audit metadata for a specific family status record.
  -->
  <app-form-drawer
    v-model="isOpen"
    title="View Family Status"
    :saving="loading"
    :save-label="''"
    :hide-save="true"
    @cancel="closeView"
  >
    <!-- Loading Spinner Overlay during asynchronous fetch operations -->
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Main Content Container Displaying Record Attributes -->
    <div v-else class="q-gutter-md">
      <!-- Field: Status Name -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Status Name
        </div>
        <div class="text-2e fs-4">
          {{ form.name || '—' }}
        </div>
      </div>

      <!-- Field: Tenant Name -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Tenant Name
        </div>
        <div class="text-2e fs-14">
          {{ form.tenantName || '—' }}
        </div>
      </div>

      <!-- Field: Status (Active/Inactive) -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Status
        </div>
        <q-badge :color="form.active ? 'positive' : 'grey'">
          {{ form.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Field: Created By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created By
        </div>
        <div class="text-2e fs-14">
          {{ form.createdBy || '—' }}
        </div>
      </div>

      <!-- Field: Created On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created On
        </div>
        <div class="text-2e fs-14">
          {{ form.createdOnUtc || '—' }}
        </div>
      </div>

      <!-- Field: Updated By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated By
        </div>
        <div class="text-2e fs-14">
          {{ form.updatedBy || '—' }}
        </div>
      </div>

      <!-- Field: Updated On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated On
        </div>
        <div class="text-2e fs-14">
          {{ form.updatedOnUtc || '—' }}
        </div>
      </div>
    </div>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from "vue";
import { familyStatusApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useTenantOptions } from "composables/useTenantOptions";

import AppFormDrawer from "components/common/AppFormDrawer.vue";

// Component property definitions for model binding and record identification
const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  recordId: {
    type: [String, Number],
    default: null
  }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const { tenantOptions, loadTenants } = useTenantOptions();

// Component loading and reactive form state declarations
const loading = ref(false);
const form = reactive({
  tenantName: "",
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: "",
  updatedBy: "",
  updatedOnUtc: ""
});

/*
 * Load tenant options on mount for fallback label mapping.
 */
onMounted(async () => {
  try {
    if (loadTenants) await loadTenants();
  } catch (err) {
    console.warn("Could not load tenant options:", err);
  }
});

/*
 * Computed property establishing two-way binding for drawer visibility control.
 */
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

/*
 * Watcher monitoring drawer visibility state changes to trigger record data fetching or cleanup.
 */
watch(
  () => props.modelValue,
  async (val) => {
    if (val && props.recordId) {
      await fetchRecordDetails(props.recordId);
    } else if (!val) {
      resetForm();
    }
  }
);

/*
 * Resets all reactive form fields to their default empty states.
 */
const resetForm = () => {
  form.tenantName = "";
  form.name = "";
  form.active = true;
  form.createdBy = "";
  form.createdOnUtc = "";
  form.updatedBy = "";
  form.updatedOnUtc = "";
};

/*
 * Safely closes the view drawer and clears associated form data.
 */
const closeView = () => {
  isOpen.value = false;
  resetForm();
};

/**
 * Asynchronously fetches specific family status record details from the API endpoint.
 * Handles fallback resolution for tenants, creators, updaters, and timestamps.
 * @param {String|Number} id - Target entity unique identifier.
 */
const fetchRecordDetails = async (id) => {
  loading.value = true;
  
  try {
    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      // Map status name and active state with fallbacks
      form.name = item.name || item.Name || item.familyStatusName || item.FamilyStatusName || "—";
      form.active = item.active ?? item.Active ?? true;
      
      // Resolve tenant name with robust fallback mapping matching list/detail view behavior
      let resolvedTenant = item.tenantName || item.tenant_name || item.tenant?.name;
      if (!resolvedTenant && tenantOptions?.value) {
        const tenantId = item.tenantId || item.tenantid || item.TenantId;
        if (tenantId) {
          const found = tenantOptions.value.find((t) => t.value === tenantId || t.id === tenantId);
          resolvedTenant = found ? found.label : tenantId;
        }
      }
      form.tenantName = resolvedTenant || "—";

      // Map audit metadata fields: Created By & Updated By
      form.createdBy = item.createdBy || item.CreatedBy || "—";
      form.updatedBy = item.updatedBy || item.UpdatedBy || "—";

      // Format creation date string with multiple fallbacks
      const rawCreatedDate = item.createdOnUtc || item.CreatedOnUtc || item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawCreatedDate) {
        const date = new Date(rawCreatedDate);
        form.createdOnUtc = isNaN(date.getTime()) ? String(rawCreatedDate) : date.toLocaleString();
      } else {
        form.createdOnUtc = "—";
      }

      // Format update date string with multiple fallbacks
      const rawUpdatedDate = item.updatedOnUtc || item.UpdatedOnUtc || item.updatedOn || item.UpdatedOn || item.updatedAt || item.UpdatedAt;
      if (rawUpdatedDate) {
        const date = new Date(rawUpdatedDate);
        form.updatedOnUtc = isNaN(date.getTime()) ? String(rawUpdatedDate) : date.toLocaleString();
      } else {
        form.updatedOnUtc = "—";
      }
    } else {
      notify.error("Family status record details could not be found.");
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    isOpen.value = false;
  } finally {
    loading.value = false;
  }
};
</script>