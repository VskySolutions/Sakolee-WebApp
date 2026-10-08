<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Policy"
    size="lg"
    hide-save
    @cancel="close"
  >
    <!-- Loading State Spinner -->
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <!-- Content Section -->
    <div v-else class="row q-col-gutter-lg">
      <!-- Policy Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Policy Name</div>
        <div class="text-2e fs-14">{{ viewPolicy.name || "—" }}</div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-3">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewPolicy.active ? 'active-badge' : 'inactive-badge'">
          {{ viewPolicy.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Display Order -->
      <div class="col-12 col-sm-3">
        <div class="text-86 fs-12 fw-500">Display Order</div>
        <div class="text-2e fs-14">{{ viewPolicy.displayOrder ?? 0 }}</div>
      </div>

      <!-- Description -->
      <div class="col-12">
        <div class="text-86 fs-12 fw-500">Description</div>
        <div class="text-2e fs-14" style="white-space: pre-line">{{ viewPolicy.description || "—" }}</div>
      </div>

      <!-- Classes -->
      <div class="col-12">
        <div class="text-86 fs-12 fw-500">Applies to Classes</div>
        <div v-if="viewPolicy.classIds.length" class="q-gutter-xs q-mt-xs">
          <q-chip
            v-for="classId in viewPolicy.classIds"
            :key="classId"
            dense
            color="teal-1"
            text-color="primary"
          >
            {{ classMap[classId] || "Unknown class" }}
          </q-chip>
        </div>
        <div v-else class="text-2e fs-14">—</div>
      </div>

      <!-- Content -->
      <div class="col-12">
        <app-rich-text-field
          :model-value="viewPolicy.content"
          label="Policy Content"
          readonly
        />
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">{{ viewPolicy.createdBy || "—" }}</div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">{{ formatDate(viewPolicy.createdOnUtc) }}</div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">{{ viewPolicy.updatedBy || "—" }}</div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">{{ formatDate(viewPolicy.updatedOnUtc) }}</div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { policyApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppRichTextField from "components/common/AppRichTextField.vue";

// Props and emit
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  viewId: { type: [Object, String, Number], default: null },
  // classId → class name
  classMap: { type: Object, default: () => ({}) }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

const viewPolicy = reactive({
  id: null,
  name: "",
  description: "",
  content: "",
  displayOrder: 0,
  classIds: [],
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

// Resets the view
const resetView = () => {
  viewPolicy.id = null;
  viewPolicy.name = "";
  viewPolicy.description = "";
  viewPolicy.content = "";
  viewPolicy.displayOrder = 0;
  viewPolicy.classIds = [];
  viewPolicy.active = true;
  viewPolicy.createdBy = "";
  viewPolicy.createdOnUtc = null;
  viewPolicy.updatedBy = "";
  viewPolicy.updatedOnUtc = null;
};

watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val && props.viewId) {
    await fetchDetails(props.viewId);
  } else if (!val) {
    resetView();
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

const fetchDetails = async (id) => {
  resetView();
  viewLoading.value = true;
  try {
    const item = await policyApi.get(id);

    if (item) {
      viewPolicy.id = id;
      viewPolicy.name = item.name || "";
      viewPolicy.description = item.description || "";
      viewPolicy.content = item.content || "";
      viewPolicy.displayOrder = item.displayOrder ?? 0;
      viewPolicy.classIds = item.classIds || [];
      viewPolicy.active = item.active ?? true;
      viewPolicy.createdBy = item.createdBy || "";
      viewPolicy.createdOnUtc = item.createdOnUtc || null;
      viewPolicy.updatedBy = item.updatedBy || "";
      viewPolicy.updatedOnUtc = item.updatedOnUtc || null;
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
