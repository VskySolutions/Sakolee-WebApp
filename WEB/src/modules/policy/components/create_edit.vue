<template>
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Policy' : 'Create Policy'"
    :saving="saving"
    :save-label="isEditing ? 'Save' : 'Create'"
    size="lg"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <q-form v-else ref="formRef" greedy>
      <div class="row q-col-gutter-md">
        <!-- Policy Name Field -->
        <div class="col-12 col-sm-8">
          <app-text-field
            v-model="form.name"
            label="Policy Name"
            required
            maxlength="200"
            :error="formErrors.name.hasError"
            :error-message="formErrors.name.message"
            :rules="[
              (v) => !!v?.trim() || 'Policy name is required',
              (v) => !v || v.trim().length <= 200 || 'Policy name cannot exceed 200 characters'
            ]"
            @update:model-value="formErrors.name.hasError = false"
          />
        </div>

        <!-- Display Order Field -->
        <div class="col-12 col-sm-4">
          <app-text-field
            v-model.number="form.displayOrder"
            label="Display Order"
            type="number"
            :rules="[(v) => v === '' || v === null || Number(v) >= 0 || 'Display order cannot be negative']"
          />
        </div>

        <!-- Description Field -->
        <div class="col-12">
          <app-text-field
            v-model="form.description"
            label="Description"
            type="textarea"
            autogrow
            maxlength="1000"
            :rules="[(v) => !v || v.length <= 1000 || 'Description cannot exceed 1000 characters']"
          />
        </div>

        <!-- Classes Multi-select -->
        <div class="col-12">
          <app-select
            v-model="form.classIds"
            label="Applies to Classes"
            info="The classes this policy applies to."
            :options="classOptions"
            :loading="classesLoading"
            multiple
          />
        </div>

        <!-- Policy Content (rich text) -->
        <div class="col-12">
          <app-rich-text-field
            v-model="form.content"
            label="Policy Content"
            placeholder="Write the policy text"
          />
        </div>

        <!-- Status Toggle -->
        <div class="col-12">
          <q-toggle v-model="form.active" label="Active" />
        </div>
      </div>
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, watch, computed } from "vue";
import { policyApi, getApiErrorMessage, getApiErrorCode, ApiErrorCodes } from "services/api";
import { useNotify } from "composables/useNotify";

import AppFormDialog from "components/common/AppFormDialog.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppRichTextField from "components/common/AppRichTextField.vue";

// Component props received from the parent component.
const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [Object, String, Number], default: null },
  // [{ label, value }] — every class of the tenant, so already-mapped inactive classes still show by name.
  classOptions: { type: Array, default: () => [] },
  classesLoading: { type: Boolean, default: false }
});

// Events emitted to the parent component.
const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const saving = ref(false);
const loading = ref(false);
const formRef = ref(null);

const isOpen = ref(props.modelValue);

// Check if we are in editing mode
const isEditing = computed(() => !!props.editingId);

// Reactive form data and error states
const form = reactive({
  name: "",
  description: "",
  content: "",
  displayOrder: 0,
  classIds: [],
  active: true
});

const formErrors = reactive({
  name: { hasError: false, message: "" }
});

// Function to reset form values to their default state
const resetFormValues = () => {
  form.name = "";
  form.description = "";
  form.content = "";
  form.displayOrder = 0;
  form.classIds = [];
  form.active = true;
  formErrors.name.hasError = false;
  formErrors.name.message = "";
};

// The list rows carry no content, so editing loads the full record.
const loadPolicy = async (id) => {
  loading.value = true;
  try {
    const item = await policyApi.get(id);
    form.name = item?.name || "";
    form.description = item?.description || "";
    form.content = item?.content || "";
    form.displayOrder = item?.displayOrder ?? 0;
    form.classIds = item?.classIds || [];
    form.active = item?.active ?? true;
  } catch (err) {
    notify.error(getApiErrorMessage(err));
    isOpen.value = false;
  } finally {
    loading.value = false;
  }
};

// Watcher to populate form when opened
watch(() => props.modelValue, (val) => {
  isOpen.value = val;
  if (val) {
    resetFormValues();
    if (props.editingId) {
      loadPolicy(props.editingId);
    }
  }
});

// Watcher to emit changes to the parent component
watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

// Function to reset the form and close the dialog
const resetForm = () => {
  resetFormValues();
  isOpen.value = false;
};

// Function to submit the form data to the API, handling both create and update operations
const submitForm = async ({ clearDraft } = {}) => {
  formErrors.name.hasError = false;
  formErrors.name.message = "";

  if (!(await formRef.value?.validate())) {
    return;
  }

  saving.value = true;

  try {
    const payload = {
      name: form.name.trim(),
      description: form.description?.trim() || null,
      content: form.content || null,
      displayOrder: Number(form.displayOrder) || 0,
      classIds: form.classIds || [],
      active: form.active
    };

    if (props.editingId) {
      await policyApi.update(props.editingId, payload);
      notify.success("Policy updated successfully.");
    } else {
      await policyApi.create(payload);
      notify.success("Policy created successfully.");
    }

    clearDraft?.();
    isOpen.value = false;
    resetFormValues();
    emit("saved");
  } catch (err) {
    const errCode = getApiErrorCode(err);
    const errMsg = getApiErrorMessage(err)?.toLowerCase() || "";

    // Duplicate names are shown on the field rather than as a toast.
    if (errCode === ApiErrorCodes.DuplicateIdentifier || errMsg.includes("already exists")) {
      formErrors.name.hasError = true;
      formErrors.name.message = "A policy with this name already exists.";
    } else {
      notify.error(getApiErrorMessage(err));
    }
  } finally {
    saving.value = false;
  }
};
</script>
