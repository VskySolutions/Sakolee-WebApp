<template>
  <q-dialog v-model="isOpen" persistent>
    <q-card class="column q-pa-lg relative-position" style="width: 820px; max-width: 95vw; border-radius: 16px;">
      
      <!-- Fixed Close Button -->
      <q-btn
        flat
        round
        dense
        size="lg"
        class="absolute-top-right text-grey-7"
        style="z-index: 10; margin: 16px; font-weight: bold;"
        v-close-popup
        @click="closeView"
      >
        &times;
      </q-btn>

      <!-- Modal Header Section -->
      <div class="row items-center q-mb-xl q-pr-lg">
        <div class="row items-center q-gutter-md">
          <q-avatar color="primary" text-color="white" size="48px" font-size="18px">
            {{ getInitials(viewSession.sessionName) }}
          </q-avatar>
          <div>
            <div class="text-h6 text-weight-bold text-dark q-ma-none">
              {{ viewSession.sessionName || "Session Details" }}
            </div>
            <div class="text-caption text-grey-7">
              Class Session Information & Audit Logs
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State Spinner -->
      <div v-if="viewLoading" class="row flex-center q-pa-xl col">
        <q-spinner color="primary" size="40px" />
      </div>

      <!-- Content Section -->
      <div v-else class="col scroll q-gutter-y-md">
        
        <!-- Top Metric Cards Row -->
        <div class="row q-col-gutter-md">
          <!-- Status Card -->
          <div class="col-12 col-sm-6">
            <q-card flat bordered class="q-pa-md rounded-borders bg-grey-1 column justify-between" style="min-height: 78px;">
              <div class="text-caption text-grey-7 text-weight-bold uppercase q-mb-xs" style="font-size: 11px; letter-spacing: 0.5px;">Status</div>
              <div>
                <q-badge :color="viewSession.isActive ? 'positive' : 'grey'" dense>
                  {{ viewSession.isActive ? "Active" : "Inactive" }}
                </q-badge>
              </div>
            </q-card>
          </div>

          <!-- Session Name Card -->
          <div class="col-12 col-sm-6">
            <q-card flat bordered class="q-pa-md rounded-borders bg-grey-1 column justify-between" style="min-height: 78px;">
              <div class="text-caption text-grey-7 text-weight-bold uppercase q-mb-xs" style="font-size: 11px; letter-spacing: 0.5px;">Session Name</div>
              <div class="text-body2 text-weight-bold text-dark ellipsis">
                {{ viewSession.sessionName || "—" }}
              </div>
            </q-card>
          </div>
        </div>

        <!-- Detailed Information Card with 2-Column Grid -->
        <q-card flat bordered class="q-pa-md rounded-borders">
          <div class="text-subtitle2 text-primary text-weight-bold q-mb-md row items-center q-gutter-xs">
            <q-icon name="o_info" size="20px" />
            <span>Audit & Timestamp Information</span>
          </div>

          <div class="row q-col-gutter-lg">
            <!-- Created By -->
            <div class="col-12 col-sm-6">
              <div class="text-caption text-grey-7 text-weight-medium" style="font-size: 11px;">Created By</div>
              <div class="text-body1 text-dark text-weight-medium q-mt-xs">
                {{ viewSession.createdBy || "—" }}
              </div>
            </div>

            <!-- Created On -->
            <div class="col-12 col-sm-6">
              <div class="text-caption text-grey-7 text-weight-medium" style="font-size: 11px;">Created On</div>
              <div class="text-body1 text-dark text-weight-medium q-mt-xs">
                {{ formatDate(viewSession.createdOnUtc) }}
              </div>
            </div>

            <!-- Updated By -->
            <div class="col-12 col-sm-6 q-mt-md">
              <div class="text-caption text-grey-7 text-weight-medium" style="font-size: 11px;">Updated By</div>
              <div class="text-body1 text-dark text-weight-medium q-mt-xs">
                {{ viewSession.updatedBy || "—" }}
              </div>
            </div>

            <!-- Updated On -->
            <div class="col-12 col-sm-6 q-mt-md">
              <div class="text-caption text-grey-7 text-weight-medium" style="font-size: 11px;">Updated On</div>
              <div class="text-body1 text-dark text-weight-medium q-mt-xs">
                {{ formatDate(viewSession.updatedOnUtc) }}
              </div>
            </div>
          </div>
        </q-card>

      </div>

      <!-- Footer Action Area -->
      <div class="row justify-end q-mt-lg">
        <q-btn label="Close" color="primary" unelevated class="q-px-lg" v-close-popup @click="closeView" />
      </div>

    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { classSessionApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  recordId: { type: [String, Number, null], default: null }
});

const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const viewLoading = ref(false);

const viewSession = reactive({
  id: null,
  sessionName: "",
  isActive: true,
  createdBy: "",
  createdOnUtc: null,
  updatedBy: "",
  updatedOnUtc: null
});

const isOpen = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val)
});

const getInitials = (name) => {
  if (!name) return "CS";
  const parts = name.trim().split(" ");
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
};

const formatDate = (value) => {
  if (!value) return "—";
  const date = new Date(value);
  return isNaN(date.getTime()) ? String(value) : date.toLocaleString();
};

const resetViewSession = () => {
  viewSession.id = null;
  viewSession.sessionName = "";
  viewSession.isActive = true;
  viewSession.createdBy = "";
  viewSession.createdOnUtc = null;
  viewSession.updatedBy = "";
  viewSession.updatedOnUtc = null;
};

watch(
  () => props.modelValue,
  async (val) => {
    if (val && props.recordId) {
      await fetchRecordDetails(props.recordId);
    } else if (!val) {
      resetViewSession();
    }
  }
);

const fetchRecordDetails = async (id) => {
  resetViewSession();
  viewLoading.value = true;

  try {
    const response = await classSessionApi.get(id);
    const item = response?.data?.data || response?.data || response;

    if (item) {
      viewSession.id = id;
      viewSession.sessionName = item.sessionName || item.SessionName || item.name || item.Name || "";
      viewSession.isActive = item.isActive !== undefined ? item.isActive : (item.IsActive !== undefined ? item.IsActive : true);
      viewSession.createdBy = item.createdBy || item.CreatedBy || "";
      viewSession.createdOnUtc = item.createdOnUtc || item.CreatedOnUtc || item.created_on_utc || item.createdOn || item.CreatedOn || item.created_on || null;
      viewSession.updatedBy = item.updatedBy || item.UpdatedBy || item.updated_by || "";
      viewSession.updatedOnUtc = item.updatedOnUtc || item.UpdatedOnUtc || item.updated_on_utc || item.updatedOn || item.UpdatedOn || item.updated_on || null;
    }
  } catch (err) {
    isOpen.value = false;
    notify.error(getApiErrorMessage(err));
  } finally {
    viewLoading.value = false;
  }
};

const closeView = () => {
  isOpen.value = false;
  resetViewSession();
};
</script>