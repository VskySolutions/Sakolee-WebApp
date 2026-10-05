<template>
  <app-form-dialog
    v-model="isOpen"
    :title="staff ? staff.fullName : 'Staff Details'"
    :subtitle="staffSubtitle"
    avatar-text="S"
    size="md"
    hide-save
    hide-footer
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="32px" />
    </div>

    <div v-else-if="staff" class="view-student-scss">
      <!-- SUMMARY CARDS-->
      <div class="student-summary row q-col-gutter-md q-mb-md">
        <div class="col-12">
          <div class="summary-card">
            <div class="summary-card__label">ACCOUNT STATUS</div>
            <div class="summary-card__value">
              <q-badge :color="staff.isActive ? 'positive' : 'grey'">
                {{ staff.isActive ? "Active" : "Inactive" }}
              </q-badge>
            </div>
          </div>
        </div>
      </div>

    <!-- PERSONAL / GENERAL INFORMATION -->
      <div class="row q-col-gutter-md">
        <!-- Personal / General Info -->
        <div class="col-12">
          <section class="border-80 br-16 pa-20">
            <div class="info-card__header">
              <q-icon name="o_badge" class="fs-16 text-4d" />
              <span class="text-4d fw-700 fs-12 lh-16">STAFF INFORMATION</span>
            </div>

            <div class="mt-12">
              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Full Name</span>
                <span class="fs-11 fw-600 text-2e">{{ staff.fullName || "—" }}</span>
              </div>

              <div class="info-row">
                <span class="fs-11 fw-400 text-86">Email</span>
                <span class="fs-11 fw-600 text-2e">{{ staff.email || "—" }}</span>
              </div>
            </div>
          </section>
        </div>
      </div>

      <!--AUDIT -->
      <div class="mt-24">
        <app-record-audit :audit="staff.audit" />
      </div>
    </div>
  </app-form-dialog>
</template>

<script setup>

// Import necessary modules and components

import { ref, computed, watch } from "vue";
import { userApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import AppFormDialog from "components/common/AppFormDialog.vue";
import AppRecordAudit from "components/common/AppRecordAudit.vue";

// Props for the component
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  userId: { type: String, default: null }
});
const emit = defineEmits(["update:modelValue"]);

const notify = useNotify();
const staff = ref(null);
const loading = ref(false);

// Computed property to manage the open state of the dialog
const isOpen = computed({
  get: () => props.modelValue,
  set: (v) => emit("update:modelValue", v)
});

// Computed property for the subtitle of the staff profiles
const staffSubtitle = computed(() => {
  return `Staff Member Profile & Details`;
});

// Function to load staff details based on the provided userId
const load = async () => {
  if (!props.userId) return;
  loading.value = true;
  try {
    staff.value = await userApi.get(props.userId);
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
};

// Watchers to trigger loading of staff details when modelValue or userId changes
watch(
  () => props.modelValue,
  (val) => {
    if (val) load();
  }
);

watch(
  () => props.userId,
  () => {
    if (props.modelValue) load();
  }
);
</script>