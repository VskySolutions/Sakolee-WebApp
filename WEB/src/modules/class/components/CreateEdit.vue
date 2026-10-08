<template>
  <!-- Add/Edit Class popup, opened from the All Classes list -->
  <app-form-dialog
    v-model="formOpen"
    :title="isEdit ? 'Edit Class' : 'Add Class'"
    :subtitle="isEdit
      ? 'Update the class definition, scheduling, pricing, instructors, and enrollment settings.'
      : 'Create a new class definition, scheduling, pricing, instructors, and enrollment settings.'"
    :saving="saving"
    :save-label="isEdit ? 'Update Class' : 'Save Class'"
    size="md"
    @submit="submitForm"
    @cancel="reset"
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <q-form v-else ref="formRef" greedy @submit.prevent="submitForm">
      <class-form-fields v-model="form" :show-active-toggle="isEdit" />
    </q-form>

    <template v-if="!isEdit" #footer-actions>
      <q-btn
        flat
        no-caps
        label="Save as Draft"
        :disable="saving"
        class="close-btn br-12"
        @click="saveDraft"
      />
    </template>
  </app-form-dialog>
</template>

<script setup>
import { computed, reactive, ref, watch } from "vue";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankClassForm, classFormFromRow, toClassPayload } from "composables/classForm";

import AppFormDialog from "components/common/AppFormDialog.vue";
import ClassFormFields from "components/class/ClassFormFields.vue";

// classId set → edit that class; null → add a new one.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  classId: { type: [String, Number], default: null }
});
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();

const formOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit("update:modelValue", value)
});
const isEdit = computed(() => !!props.classId);
const loading = ref(false);
const saving = ref(false);
const formRef = ref(null);
const form = reactive(blankClassForm());

// "Save as Draft" keeps an unfinished new class in this browser (there is no server-side draft status);
// it is restored the next time Add Class opens and cleared once the class is saved.
const DRAFT_KEY = "classFormDraft";
const readDraft = () => {
  try {
    const raw = localStorage.getItem(DRAFT_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
};
const writeDraft = (value) => {
  try {
    localStorage.setItem(DRAFT_KEY, JSON.stringify(value));
    return true;
  } catch {
    return false;
  }
};
const clearDraft = () => {
  try {
    localStorage.removeItem(DRAFT_KEY);
  } catch {
    // storage unavailable — nothing to clear
  }
};

const reset = () => {
  Object.assign(form, blankClassForm());
  formRef.value?.resetValidation();
};

const loadForm = async () => {
  reset();
  if (!isEdit.value) {
    const draft = readDraft();
    if (draft) {
      Object.assign(form, draft);
      notify.info("Restored your saved class draft.");
    }
    return;
  }
  loading.value = true;
  try {
    const row = await classApi.get(props.classId);
    Object.assign(form, classFormFromRow(row));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    formOpen.value = false;
  } finally {
    loading.value = false;
  }
};

watch(() => props.modelValue, (open) => { if (open) loadForm(); });

const saveDraft = () => {
  if (writeDraft({ ...form })) {
    notify.success("Class draft saved.");
    formOpen.value = false;
  } else {
    notify.error("Unable to save the draft in this browser.");
  }
};

const validateInstructorSchedule = async () => {
  const instructorIds = [
    form.primaryInstructor,
    ...(form.additionalInstructors || [])
  ].filter(Boolean);
  if (!instructorIds.length) {
    return true;
  }
  if (!form.activeDays) {
    return true;
  }
  if (!form.startDate || !form.endDate) {
    return true;
  }
  if (!form.startTime || !form.endTime) {
    return true;
  }
  const selectedDays = normalizeDays(form.activeDays);
  for (const instructorId of instructorIds) {
    const response = await classApi.listByInstructor(
      instructorId,
      {
        page: 1,
        limit: 100,
        descending: true
      }
    );
    const classes = response?.data || [];
    for (const existingClass of classes) {
      // Ignore the class currently being edited.
      if (
        isEdit.value &&
        String(existingClass.classId) === String(props.classId)
      ) {
        continue;
      }
      // Inactive classes do not create schedule conflicts.
      const status = String(existingClass.status || "").toLowerCase();
      if (status !== "active") {
        continue;
      }
      // Check date range.
      const dateOverlap = hasDateOverlap(
        existingClass.startDate,
        existingClass.endDate,
        form.startDate,
        form.endDate
      );
      if (!dateOverlap) {
        continue;
      }
      // Check active days.
      const dayOverlap = hasCommonActiveDay(
        existingClass.activeDays,
        selectedDays
      );
      if (!dayOverlap) {
        continue;
      }
      // Convert existing "times" to start/end.
      const existingTimes = parseExistingTimes(
        existingClass.times
      );
      // Check time overlap.
      const timeOverlap = hasTimeOverlap(
        existingTimes.startTime,
        existingTimes.endTime,
        form.startTime,
        form.endTime
      );
      if (!timeOverlap) {
        continue;
      }
      return {
        valid: false,
        message:
          `Instructor is already scheduled for "${existingClass.className}" ` +
          `during an overlapping day and time.`
      };
    }
  }

  return true;
};
const timeToMinutes = (value) => {
  if (!value) return null;
  const text = String(value).trim();
  // 24-hour format: HH:mm
  let match = text.match(/^(\d{1,2}):(\d{2})$/);
  if (match) {
    const hours = Number(match[1]);
    const minutes = Number(match[2]);
    if (hours >= 0 && hours <= 23 && minutes >= 0 && minutes <= 59) {
      return hours * 60 + minutes;
    }
  }
  // 12-hour format: h:mm AM/PM
  match = text.match(/^(\d{1,2}):(\d{2})\s*(AM|PM)$/i);
  if (match) {
    let hours = Number(match[1]);
    const minutes = Number(match[2]);
    const period = match[3].toUpperCase();
    if (hours >= 1 && hours <= 12 && minutes >= 0 && minutes <= 59) {
      if (period === "AM") {
        if (hours === 12) hours = 0;
      } else if (hours !== 12) {
        hours += 12;
      }
      return hours * 60 + minutes;
    }
  }
  return null;
};

const parseExistingTimes = (times) => {
  if (!times) {
    return {
      startTime: null,
      endTime: null
    };
  }
  const text = String(times).trim();
  const parts = text.split(/\s*[-–—]\s*/);
  if (parts.length < 2) {
    return {
      startTime: null,
      endTime: null
    };
  }
  return {
    startTime: parts[0].trim(),
    endTime: parts[1].trim()
  };
};

const normalizeDays = (value) => {
  if (!value) return [];
  if (Array.isArray(value)) {
    return value
      .map((day) => String(day).trim().toLowerCase())
      .filter(Boolean);
  }
  return String(value)
    .split(",")
    .map((day) => day.trim().toLowerCase())
    .filter(Boolean);
};

const hasCommonActiveDay = (existingDays, selectedDays) => {
  const existing = normalizeDays(existingDays);
  const selected = normalizeDays(selectedDays);
  return selected.some((day) => existing.includes(day));
};

const normalizeDate = (value) => {
  if (!value) return null;
  const text = String(value).slice(0, 10);
  return text.replaceAll("/", "-");
};
const hasDateOverlap = (
  existingStartDate,
  existingEndDate,
  selectedStartDate,
  selectedEndDate
) => {
  const existingStart = normalizeDate(existingStartDate);
  const existingEnd = normalizeDate(existingEndDate);
  const selectedStart = normalizeDate(selectedStartDate);
  const selectedEnd = normalizeDate(selectedEndDate);
  if (
    !existingStart ||
    !existingEnd ||
    !selectedStart ||
    !selectedEnd
  ) {
    return false;
  }
  return (
    existingStart <= selectedEnd &&
    existingEnd >= selectedStart
  );
};

const hasTimeOverlap = (
  existingStartTime,
  existingEndTime,
  selectedStartTime,
  selectedEndTime
) => {
  const existingStart = timeToMinutes(existingStartTime);
  const existingEnd = timeToMinutes(existingEndTime);
  const selectedStart = timeToMinutes(selectedStartTime);
  const selectedEnd = timeToMinutes(selectedEndTime);
  if (
    existingStart === null ||
    existingEnd === null ||
    selectedStart === null ||
    selectedEnd === null
  ) {
    return false;
  }
  return (
    existingStart < selectedEnd &&
    existingEnd > selectedStart
  );
};
// const submitForm = async () => {
//   if (saving.value || loading.value) return;
//   const valid = await formRef.value?.validate();
//   if (!valid) return;

//   saving.value = true;
//   try {
//     if (isEdit.value) {
//       await classApi.update(props.classId, { ...toClassPayload(form), active: form.active });
//       notify.success("Class updated.");
//     } else {
//       await classApi.create(toClassPayload(form));
//       clearDraft();
//       notify.success("Class created.");
//     }
//     formOpen.value = false;
//     reset();
//     emit("saved");
//   } catch (err) {
//     notify.error(getApiErrorMessage(err));
//   } finally {
//     saving.value = false;
//   }
// };
const submitForm = async () => {
  if (saving.value || loading.value) return;
  // Run all normal form validations first.
  const valid = await formRef.value?.validate();
  if (!valid) {
    return;
  }
  // Active Days is handled by the custom day buttons.
  if (!form.activeDays || !String(form.activeDays).trim()) {
    notify.error("Please select at least one active day.");
    return;
  }
  // Extra protection for Start/End Time.
  const startMinutes = timeToMinutes(form.startTime);
  const endMinutes = timeToMinutes(form.endTime);
  if (startMinutes === null) {
    notify.error("Please enter a valid start time.");
    return;
  }
  if (endMinutes === null) {
    notify.error("Please enter a valid end time.");
    return;
  }
  if (startMinutes >= endMinutes) {
    notify.error("End time must be after start time.");
    return;
  }
  saving.value = true;
  try {
    // Check instructor schedule before saving.
    const scheduleValidation =
      await validateInstructorSchedule();
    if (
      scheduleValidation &&
      scheduleValidation.valid === false
    ) {
      notify.error(scheduleValidation.message);
      return;
    }
    if (isEdit.value) {
      await classApi.update(
        props.classId,
        {
          ...toClassPayload(form),
          active: form.active
        }
      );
      notify.success("Class updated.");
    } else {
      await classApi.create(
        toClassPayload(form)
      );
      clearDraft();
      notify.success("Class created.");
    }
    formOpen.value = false;
    reset();
    emit("saved");
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>
