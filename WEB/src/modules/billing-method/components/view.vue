<template>
  <app-form-drawer
    v-model="isOpen"
    title="View Billing Method"
    :saving="viewLoading"
    :save-label="''"
    :hide-save="true"
    @cancel="closeView"
  >
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <div v-else class="q-gutter-md">
      <div>
        <div class="text-86 fs-12 fw-500">Billing Method Name</div>
        <div class="text-2e fs-4">
          {{ viewBillingMethod.name || '—' }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :color="viewBillingMethod.active ? 'positive' : 'grey'">
          {{ viewBillingMethod.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewBillingMethod.createdBy || "—" }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewBillingMethod.createdOnUtc) }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewBillingMethod.updatedBy || "—" }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewBillingMethod.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { billingMethodApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const viewLoading = ref(false);

// Computed property to manage the open state of the view drawer
const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

// Reactive object to hold the billing method data for viewing
const viewBillingMethod = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// Function to format date values for display in the view drawer
const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

// Function to reset the viewBillingMethod object to its initial state
const resetViewBillingMethod = () => {
  viewBillingMethod.id = null;
  viewBillingMethod.name = "";
  viewBillingMethod.active = true;
  viewBillingMethod.createdBy = "";
  viewBillingMethod.createdOnUtc = null;
  viewBillingMethod.updatedBy = "";
  viewBillingMethod.updatedOnUtc = null;
};

// Watcher to handle changes in the modelValue prop and fetch billing method data when necessary
watch(
  () => props.modelValue,
  async (val) => {
    if (val && props.recordId) {
      resetViewBillingMethod();
      viewLoading.value = true;
      try {
        const response = await billingMethodApi.get(props.recordId);
        const item = response?.data?.data || response?.data || response;

        if (item) {
          viewBillingMethod.id = props.recordId;
          viewBillingMethod.name = item.name || item.Name || item.billingMethodName || item.BillingMethodName || "";
          viewBillingMethod.active = item.active ?? item.Active ?? item.is_active ?? true;
          viewBillingMethod.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
          viewBillingMethod.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
          viewBillingMethod.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
          viewBillingMethod.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
        }
      } catch (err) {
        isOpen.value = false;
        notify.error(getApiErrorMessage(err));
      } finally {
        viewLoading.value = false;
      }
    } else if (!val) {
      resetViewBillingMethod();
    }
  }
);

// Function to close the view drawer and reset the viewBillingMethod object
const closeView = () => {
  isOpen.value = false;
  resetViewBillingMethod();
};
</script>