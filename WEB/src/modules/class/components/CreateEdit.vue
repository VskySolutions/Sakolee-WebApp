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

const submitForm = async () => {
  if (saving.value || loading.value) return;
  const valid = await formRef.value?.validate();
  if (!valid) return;

  saving.value = true;
  try {
    if (isEdit.value) {
      await classApi.update(props.classId, { ...toClassPayload(form), active: form.active });
      notify.success("Class updated.");
    } else {
      await classApi.create(toClassPayload(form));
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
