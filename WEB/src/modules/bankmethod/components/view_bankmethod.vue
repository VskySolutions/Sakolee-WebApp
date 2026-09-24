<template>
  <!-- Side-Drawer Dialog Component: View Billing Method Details Modal -->
  <q-dialog v-model="isOpen" position="right">
    <q-card class="column shadow-24 rounded-borders" style="width: 420px; max-width: 90vw; height: auto; max-height: 85vh;">
      
      <!-- Dialog Header Banner -->
      <q-card-section class="row items-center justify-between bg-primary text-white q-px-md q-py-sm">
        <div class="text-h6 text-weight-bold row items-center q-gutter-sm">
          <q-icon name="o_visibility" size="22px" />
          <div>Billing Method Details</div>
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
          <!-- Field: Billing Method Name -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Method Name:</div>
            <div class="col-7 text-dark">{{ form.name || '-' }}</div>
          </div>
          <q-separator />

          <!-- Field: Active Status -->
          <div class="row items-center">
            <div class="col-5 text-weight-bold text-grey-7">Status:</div>
            <div class="col-7 text-dark">{{ form.active ? 'Active' : 'Inactive' }}</div>
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
import { billingMethodApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

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

// Local drawer visibility synchronization with parent component
const isOpen = ref(props.modelValue);
watch(() => props.modelValue, (val) => {
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
  name: "",
  active: true,
  createdOnUtc: ""
});

/**
 * Fetches specific billing method record details from the API endpoint for viewing.
 * @param {String|Number} id - Target entity identifier.
 */
const fetchRecordDetails = async (id) => {
  loading.value = true;
  try {
    const response = await billingMethodApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      form.name = item.name || item.Name || item.billingMethodName || item.BillingMethodName || "-";
      form.active = item.active !== undefined ? item.active : (item.Active !== undefined ? item.Active : true);

      // Format creation date string with multiple fallbacks
      const rawDate = item.createdOnUtc || item.CreatedOnUtc || item.createdOn || item.CreatedOn || item.createdAt || item.CreatedAt;
      if (rawDate) {
        const date = new Date(rawDate);
        form.createdOnUtc = isNaN(date.getTime()) ? String(rawDate) : date.toLocaleString();
      } else {
        form.createdOnUtc = "-";
      }
    } else {
      notify.error("Billing method record details could not be found.");
    }
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};
</script>