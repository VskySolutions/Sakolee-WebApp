<template>
  <app-form-drawer
    v-model="isOpen"
    title="View Class Room"
    :saving="viewLoading"
    :save-label="''"
    :hide-save="true"
    @cancel="closeView"
  >
    <!-- Loading Spinner Overlay -->
    <div v-if="viewLoading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <div v-else class="q-gutter-md">
      <!-- Field: Location Name -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Location
        </div>
        <div class="text-2e fs-14">
          {{ resolveLocationName(viewClassRoom) }}
        </div>
      </div>

      <!-- Field: Class Room Name -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Class Room Name
        </div>
        <div class="text-2e fs-4">
          {{ viewClassRoom.name || '—' }}
        </div>
      </div>

      <!-- Field: Status -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Status
        </div>
        <q-badge :color="viewClassRoom.active ? 'positive' : 'grey'">
          {{ viewClassRoom.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <!-- Field: Created By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created By
        </div>
        <div class="text-2e fs-14">
          {{ viewClassRoom.createdBy || "—" }}
        </div>
      </div>

      <!-- Field: Created On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Created On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewClassRoom.createdOnUtc) }}
        </div>
      </div>

      <!-- Field: Updated By -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated By
        </div>
        <div class="text-2e fs-14">
          {{ viewClassRoom.updatedBy || "—" }}
        </div>
      </div>

      <!-- Field: Updated On -->
      <div>
        <div class="text-86 fs-12 fw-500">
          Updated On
        </div>
        <div class="text-2e fs-14">
          {{ formatDate(viewClassRoom.updatedOnUtc) }}
        </div>
      </div>
    </div>
  </app-form-drawer>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
import { classRoomApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDrawer from "components/common/AppFormDrawer.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  viewId: { type: [String, Number], default: null },
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

watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val && props.viewId) {
    await fetchDetails(props.viewId);
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

const fetchDetails = async (id) => {
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

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

const closeView = () => {
  isOpen.value = false;
};
</script>