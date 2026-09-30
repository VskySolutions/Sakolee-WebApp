<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Family Relation"
    size="sm"
    hide-save
    @cancel="close"
  >
    <!-- Loading State Spinner -->
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Content Section -->
    <div v-else class="row q-col-gutter-lg">
      <!-- Relation Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Relation Name</div>
        <div class="text-2e fs-14">
          {{ viewRelation.name || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewRelation.active ? 'active-badge' : 'inactive-badge'">
          {{ viewRelation.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewRelation.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewRelation.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewRelation.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewRelation.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { familyRelationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";

// Props and Emits
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

// Reactive object to hold the relation data
const viewRelation = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

// Handles date formatting for createdOnUtc and updatedOnUtc fields
const formatDate = (value) => {
  if (!value) return "—";
  return new Date(value).toLocaleString();
};

// Function to reset the viewRelation object to its initial state
const resetViewRelation = () => {
  viewRelation.id = null;
  viewRelation.name = "";
  viewRelation.active = true;
  viewRelation.createdBy = "";
  viewRelation.createdOnUtc = null;
  viewRelation.updatedBy = "";
  viewRelation.updatedOnUtc = null;
};

// Watchers to handle prop changes and fetch data when necessary
watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val && props.recordId) {
    await fetchRelation(props.recordId);
  } else if (!val) {
    resetViewRelation();
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to fetch the relation data from the API based on the provided recordId
const fetchRelation = async (id) => {
  resetViewRelation();
  viewLoading.value = true;

  try {
    const relation = await familyRelationApi.get(id);

    viewRelation.id = relation?.id;
    viewRelation.name = relation?.name || "";
    viewRelation.active = relation?.active ?? true;
    viewRelation.createdBy = relation?.createdBy || "";
    viewRelation.createdOnUtc = relation?.createdOnUtc || null;
    viewRelation.updatedBy = relation?.updatedBy || "";
    viewRelation.updatedOnUtc = relation?.updatedOnUtc || null;
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

// Function to close the dialog and reset the viewRelation object
const close = () => {
  isOpen.value = false;
  resetViewRelation();
};
</script>