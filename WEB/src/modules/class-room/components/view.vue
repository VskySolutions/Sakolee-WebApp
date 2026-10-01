<template>
  <app-form-dialog
    v-model="isOpen"
    title="View Class Room"
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
      <!-- Location -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Location</div>
        <div class="text-2e fs-14">
          {{ resolveLocationName(viewClassRoom) }}
        </div>
      </div>

      <!-- Class Room Name -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Class Room Name</div>
        <div class="text-2e fs-14">
          {{ viewClassRoom.name || "—" }}
        </div>
      </div>

      <!-- Status -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="viewClassRoom.active ? 'active-badge' : 'inactive-badge'">
          {{ viewClassRoom.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Created By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">
          {{ viewClassRoom.createdBy || "—" }}
        </div>
      </div>

      <!-- Created On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewClassRoom.createdOnUtc) }}
        </div>
      </div>

      <!-- Updated By -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">
          {{ viewClassRoom.updatedBy || "—" }}
        </div>
      </div>

      <!-- Updated On -->
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">
          {{ formatDate(viewClassRoom.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { classRoomApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";

//Props and emit
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  viewId: { type: [Object, String, Number], default: null },
  locationMap: { type: Object, default: () => ({}) }
});


const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const isOpen = ref(props.modelValue);
const viewLoading = ref(false);

const viewClassRoom = reactive({
  id: null,
  locationId: null,
  locationName: "",
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

//Formats the Date
// const formatDate = (value) => {
//   if (!value) return "—";
//   const date = new Date(value);
//   return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
// };

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  // Check if date is invalid or the default .NET MinValue (0001-01-01)
  if (isNaN(date.getTime()) || date.getFullYear() <= 1) return "—";
  return date.toLocaleString();
};

//Resets the view
const resetView = () => {
  viewClassRoom.id = null;
  viewClassRoom.locationId = null;
  viewClassRoom.locationName = "";
  viewClassRoom.name = "";
  viewClassRoom.active = true;
  viewClassRoom.createdBy = "";
  viewClassRoom.createdOnUtc = null;
  viewClassRoom.updatedBy = "";
  viewClassRoom.updatedOnUtc = null;
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
    const response = await classRoomApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewClassRoom.id = id;
      viewClassRoom.locationId = item.locationId || item.LocationId || null;
      viewClassRoom.locationName = item.locationName || item.LocationName || item.location?.name || "";
      viewClassRoom.name = item.name || item.Name || "";
      viewClassRoom.active = item.active ?? item.Active ?? true;
      viewClassRoom.createdBy = item.createdBy || item.CreatedBy || "";
      viewClassRoom.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || null;
      viewClassRoom.updatedBy = item.updatedBy || item.UpdatedBy || "";
      viewClassRoom.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || null;
    }
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};


const resolveLocationName = (row) => {
  if (!row) return "—";
  const directName = row.locationName || row.LocationName;
  if (directName) return directName;

  const locId = row.locationId || row.LocationId;
  if (locId && props.locationMap[locId]) {
    return props.locationMap[locId];
  }

  return "—";
};

const close = () => {
  isOpen.value = false;
  resetView();
};
</script>