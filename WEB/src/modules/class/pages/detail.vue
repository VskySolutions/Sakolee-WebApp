<template>
  <q-page padding>
    <app-detail-header
      :items="[
        { label: 'Home', icon: 'o_home', to: '/' },
        { label: 'Classes', to: { name: 'classes' } },
        { label: 'View Class' }
      ]"
      :back-to="{ name: 'classes' }"
    />

    <div v-if="loading" class="row flex-center q-pa-xl">
      <q-spinner color="primary" size="40px" />
    </div>

    <q-card v-else flat bordered class="q-pa-md">
      <q-form class="q-gutter-md">
        <class-form-fields v-model="form" disable show-active-toggle />

        <div class="row justify-end q-mt-md">
          <q-btn unelevated no-caps color="primary" label="Back" :to="{ name: 'classes' }" />
        </div>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup>
import { onMounted, reactive, ref } from "vue";
import { useRoute } from "vue-router";
import { classApi, getApiErrorMessage } from "services/api";
import { useNotify } from "composables/useNotify";
import { blankClassForm, classFormFromRow } from "composables/classForm";

import AppDetailHeader from "components/common/AppDetailHeader.vue";
import ClassFormFields from "components/class/ClassFormFields.vue";

const route = useRoute();
const notify = useNotify();

const classId = route.params.id;
const loading = ref(false);
const form = reactive(blankClassForm());

onMounted(async () => {
  loading.value = true;
  try {
    const row = await classApi.get(classId);
    Object.assign(form, classFormFromRow(row));
  } catch (err) {
    notify.error(getApiErrorMessage(err));
  } finally {
    loading.value = false;
  }
});
</script>
