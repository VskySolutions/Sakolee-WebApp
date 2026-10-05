<template>
  <app-form-dialog
    v-model="isOpen"
    :title="isEditing ? 'Edit Person' : 'Create Person'"
    :saving="saving"
    :save-label="isEditing ? 'Save Changes' : 'Create Person'"
    size="md"
    @submit="submitForm"
    @cancel="resetForm"
  >
    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <q-form v-else ref="formRef" greedy>
      <person-form-fields
        v-model="form"
        :tenant-options="showTenantPicker ? tenantOptions : []"
        :loading-tenants="loadingTenants"
      />
    </q-form>
  </app-form-dialog>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { personApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankPersonForm } from "composables/personForm";
import { useTenantOptions } from "composables/useTenantOptions";
import AppFormDialog from "components/common/AppFormDialog.vue";
import PersonFormFields from "components/person/PersonFormFields.vue";

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  editingId: { type: [String, Number], default: null },
  tenantId: { type: String, default: null }
});

const emit = defineEmits(["update:modelValue", "saved"]);

const notify = useNotify();
const formRef = ref(null);
const saving = ref(false);
const loading = ref(false);
const form = reactive(blankPersonForm());
const { canChooseTenant, activeTenantId, tenantOptions, loadingTenants, loadTenants } = useTenantOptions();

const isOpen = ref(props.modelValue);
const isEditing = computed(() => !!props.editingId);
const showTenantPicker = computed(() => canChooseTenant.value && !props.tenantId);

watch(() => props.modelValue, async (val) => {
  isOpen.value = val;
  if (val) {
    if (canChooseTenant.value) {
      await loadTenants();
    }

    if (props.editingId) {
      loading.value = true;
      try {
        const detail = await personApi.get(props.editingId);
        const p = detail.profile || detail; // Server response structure handle karne ke liye
        
        form.tenantIds = p.tenantIds || p.TenantIds || [];
        form.suffix = p.suffix || p.Suffix || "";
        form.firstName = p.firstName || p.FirstName || "";
        form.middleName = p.middleName || p.MiddleName || "";
        form.lastName = p.lastName || p.LastName || "";
        form.preferredName = p.preferredName || p.PreferredName || "";
        form.displayName = p.displayName || p.DisplayName || "";
        form.gender = p.gender || p.Gender || null;
        
        const dob = p.dateOfBirth || p.DateOfBirth;
        form.dateOfBirth = dob ? dob.substring(0, 10) : "";
        
        form.primaryEmail = p.primaryEmail || p.PrimaryEmail || "";
        form.secondaryEmail = p.secondaryEmail || p.SecondaryEmail || "";
        form.mobileNumber = p.mobileNumber || p.MobileNumber || "";
        form.countryCode = p.countryCode || p.CountryCode || null;
        form.alternateMobileNumber = p.alternateMobileNumber || p.AlternateMobileNumber || "";
        form.employeeCode = p.employeeCode || p.EmployeeCode || "";
      } catch (err) {
        notify.error(getApiErrorMessage(err));
      } finally {
        loading.value = false;
      }
    } else {
      Object.assign(form, blankPersonForm());
      if (props.tenantId) {
        form.tenantIds = [props.tenantId];
      } else {
        form.tenantIds = [activeTenantId.value];
      }
    }
  }
});

watch(isOpen, (val) => {
  emit("update:modelValue", val);
});

const resetForm = () => {
  Object.assign(form, blankPersonForm());
  isOpen.value = false;
};

const submitForm = async ({ clearDraft } = {}) => {
  if (!(await formRef.value?.validate())) return;
  saving.value = true;
  try {
    const payload = { ...form, dateOfBirth: form.dateOfBirth || null };
    if (isEditing.value) {
      await personApi.update(props.editingId, payload);
      notify.success("Person updated successfully.");
    } else {
      await personApi.create(payload);
      notify.success("Person created successfully.");
    }
    clearDraft?.();
    isOpen.value = false;
    emit("saved");
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    saving.value = false;
  }
};
</script>