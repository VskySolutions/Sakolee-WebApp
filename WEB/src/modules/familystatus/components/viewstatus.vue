<template>
  <!-- Side-Drawer Dialog Component: View Family Status Details Modal -->
  <q-dialog v-model="isOpen" position="right">
    <q-card class="column shadow-24 rounded-borders" style="width: 420px; max-width: 90vw; height: auto; max-height: 85vh;">
      
      <!-- Dialog Header Banner -->
      <q-card-section class="row items-center justify-between bg-primary text-white q-px-md q-py-sm">
        <div class="text-h6 text-weight-bold row items-center q-gutter-sm">
          <q-icon name="o_visibility" size="22px" />
          <div>Family Status Details</div>
        </div>
        <q-btn icon="o_close" flat round dense v-close-popup />
      </q-card-section>

      <!-- Dialog Scrollable Body Content -->
      <q-card-section class="col q-pa-md q-gutter-md scroll">
        <!-- Loading Spinner Overlay during fetch operations -->
        <div v-if="loading" class="row flex-center q-pa-xl">
          <q-spinner color="primary" size="40px" />
        </div>

        <template v-else>
          <!-- Field: Tenant Name ->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Tenant Name:</div>
            <div class="col-7 text-dark">{{ form.tenantName || '-' }}</div>
          </div-->
          <q-separator />

          <!-- Field: Status Name -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Status Name:</div>
            <div class="col-7 text-dark">{{ form.name || '-' }}</div>
          </div>
          <q-separator />

          <!-- Field: Created Date -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Created On:</div>
            <div class="col-7 text-dark">{{ form.createdOnUtc || '-' }}</div>
          </div>
        </template>
      </q-card-section>

      <!-- Dialog Footer Action Triggers -->
      <q-card-actions align="right" class="q-pa-sm bg-grey-1">
        <q-btn flat label="Close" color="primary" v-close-popup />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { familyStatusApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { useTenantOptions } from "composables/useTenantOptions";

// Props definition to manage modal visibility and target record ID
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

// Local drawer visibility synchronization with parent component
const isOpen = ref(props.modelValue);
watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val && props.recordId) {
    fetchRecordDetails(props.recordId);
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Component state references
const loading = ref(false);
const form = reactive({
  tenantName: "",
  name: "",
  createdOnUtc: ""
});

/**
 * Fetches specific family status record details from the API endpoint for viewing.
 * @param {String|Number} id - Target entity identifier.
 */
const fetchRecordDetails = async (id) => {
  loading.value = true;
  
  try {
    // Attempt loading tenant options safely prior to mapping
    try {
      if (loadTenants) await loadTenants();
    } catch (tenantErr) {
      console.warn("Could not load tenant options:", tenantErr);
    }

    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      // Map status name with multi-property fallbacks
      form.name = item.name || item.Name || item.familyStatusName || item.FamilyStatusName || "-";
      
      // Resolve tenant name with robust fallback mapping matching list/detail view behavior
      let resolvedTenant = item.tenantName || item.tenant_name || item.tenant?.name;
      if (!resolvedTenant && tenantOptions?.value) {
        const tenantId = item.tenantId || item.tenantid || item.TenantId;
        if (tenantId) {
          const found = tenantOptions.value.find((t) => t.value === tenantId || t.id === tenantId);
          resolvedTenant = found ? found.label : tenantId;
        }
      }
      form.tenantName = resolvedTenant || "-";

      // Format creation date string with multiple fallbacks
      const rawDate = item.createdOnUtc || item.CreatedOnUtc || item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawDate) {
        const date = new Date(rawDate);
        form.createdOnUtc = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        form.createdOnUtc = "-";
      }
    } else {
      notify.error("Family status record details could not be found.");
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};
</script>
