<template>
  <app-form-drawer
    v-model="isOpen"
    title="View Family Status"
    :saving="viewLoading"
    :save-label="''"
    :hide-save="true"
    @cancel="close"
  >
  <!--view form content-->
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <div v-else class="q-gutter-md">
      <div>
        <div class="text-86 fs-12 fw-500">
          Family Status Name
        </div>
        <div class="text-2e fs-4">
          {{ viewFamilyStatus.familyStatusName || '—' }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">
          Status
        </div>
        <q-badge :color="viewFamilyStatus.active ? 'positive' : 'grey'">
          {{ viewFamilyStatus.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">
          Created By
        </div>
        <div class="text-2e fs-14">
          {{ viewFamilyStatus.createdBy || "—" }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">
          Created On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewFamilyStatus.createdOnUtc) }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">
          Updated By
        </div>
        <div class="text-2e fs-14">
          {{ viewFamilyStatus.updatedBy || "—" }}
        </div>
      </div>

      <div>
        <div class="text-86 fs-12 fw-500">
          Updated On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewFamilyStatus.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { familyStatusApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";

// Props and emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [Object, String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

// Reactive object to hold the family status data for viewing
const viewFamilyStatus = reactive({
  id: null,
  familyStatusName: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// Utility function to format date
const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

// Function to reset the view state
const resetView = () => {
  viewFamilyStatus.id = null;
  viewFamilyStatus.familyStatusName = "";
  viewFamilyStatus.active = true;
  viewFamilyStatus.createdBy = "";
  viewFamilyStatus.createdOnUtc = null;
  viewFamilyStatus.updatedBy = "";
  viewFamilyStatus.updatedOnUtc = null;
};

// Watchers to handle prop changes and open/close state
watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  // When the drawer is opened and a recordId is provided, fetch the record data
  if (val && props.recordId) {
    // Fetch the record data when the drawer is opened and a recordId is provided
    await fetchRecord(props.recordId);
  } else if (!val) {
    resetView();
  }
});

// Watch for changes in the isOpen state to emit updates to the parent component
watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to fetch a record by ID
const fetchRecord = async (id) => {
  resetView();
  viewLoading.value = true;
  try {
    const response = await familyStatusApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      // Populate the viewFamilyStatus reactive object with the fetched data
      viewFamilyStatus.id = id;
      viewFamilyStatus.familyStatusName = item.familyStatusName || item.FamilyStatusName || item.name || item.Name || "";
      viewFamilyStatus.active = item.active ?? item.Active ?? item.is_active ?? true;
      viewFamilyStatus.createdBy = item.createdBy || item.CreatedBy || item.created_by || "";
      viewFamilyStatus.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewFamilyStatus.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewFamilyStatus.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
    }
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

const close = () => {
  isOpen.value = false;
  resetView();
};
</script>