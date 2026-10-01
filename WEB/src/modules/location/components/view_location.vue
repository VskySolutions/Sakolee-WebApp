<template>
  <app-form-dialog v-model="dialogOpen" title="View Location" size="sm" hide-save @cancel="closeDialog">
    <div v-if="loading" class="flex flex-center q-pa-xl">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else class="row q-col-gutter-lg">
      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Name</div>
        <div class="text-2e fs-14">{{ location.name || "—" }}</div>
      </div>

      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Status</div>
        <q-badge :class="location.active ? 'active-badge' : 'inactive-badge'">
          {{ location.active ? "Active" : "Inactive" }}
        </q-badge>
      </div>

      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created By</div>
        <div class="text-2e fs-14">{{ location.createdBy || "—" }}</div>
      </div>

      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Created On</div>
        <div class="text-2e fs-14">{{ formatDate(location.createdOnUtc) }}</div>
      </div>

      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated By</div>
        <div class="text-2e fs-14">{{ location.updatedBy || "—" }}</div>
      </div>

      <div class="col-12 col-sm-6">
        <div class="text-86 fs-12 fw-500">Updated On</div>
        <div class="text-2e fs-14">{{ formatDate(location.updatedOnUtc) }}</div>
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";

import { locationApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  id: { type: [String, Number], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();

const dialogOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});

const loading = ref(false);

const location = reactive({
  id: null,
  name: "",
  active: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const resetLocation = () => {
  location.id = null;
  location.name = "";
  location.active = true;
  location.createdBy = "";
  location.createdOnUtc = null;
  location.updatedBy = "";
  location.updatedOnUtc = null;
};

const loadLocation = async () => {
  if (!props.id) {
    resetLocation();
    return;
  }

  loading.value = true;

  try {
    const response = await locationApi.get(props.id);

    location.id = response?.id || null;
    location.name = response?.name || "";
    location.active = response?.active ?? true;
    location.createdBy = response?.createdBy || "";
    location.createdOnUtc = response?.createdOnUtc || null;
    location.updatedBy = response?.updatedBy || "";
    location.updatedOnUtc = response?.updatedOnUtc || null;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    dialogOpen.value = false;
  } finally {
    loading.value = false;
  }
};

const closeDialog = () => {
  dialogOpen.value = false;
  resetLocation();
};

const formatDate = (value) => {
  if (!value) {
    return "—";
  }

  return new Date(value).toLocaleString();
};

watch(
  () => props.modelValue,
  (value) => {
    if (value) {
      loadLocation();
    } else {
      resetLocation();
    }
  }
);

watch(
  () => props.id,
  () => {
    if (props.modelValue) {
      loadLocation();
    }
  }
);
</script>
