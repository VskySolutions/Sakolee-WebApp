<template>
  <q-card flat bordered class="panel">
    <q-card-section class="row items-center no-wrap q-pb-sm">
      <div class="col panel__title">Announcements &amp; Studio Updates</div>
      <q-btn flat dense no-caps size="sm" color="primary" class="panel__action" icon="o_add" label="Post New" @click="$emit('post')" />
    </q-card-section>

    <q-separator />

    <q-card-section class="column q-gutter-sm">
      <div v-if="!rows.length" class="text-grey-6 text-center q-pa-md">No announcements posted.</div>

      <article v-for="item in rows" v-else :key="item.id" class="note">
        <div class="row items-start no-wrap">
          <div class="col note__title">{{ item.title }}</div>
          <div v-if="item.postedOn" class="note__date">{{ formatDate(item.postedOn) }}</div>
        </div>
        <p class="note__body">{{ item.body }}</p>
      </article>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  // [{ id, title, body, postedOn }]
  announcements: { type: Array, default: () => [] }
});

defineEmits(["post"]);

const rows = computed(() => props.announcements || []);

// postedOn is an ISO calendar date ("YYYY-MM-DD"), so it is split rather than passed to Date(),
// which would read it as UTC midnight and shift the day in western time zones.
const formatDate = (iso) => {
  const [y, m, d] = String(iso).split("-").map(Number);
  if (!y || !m || !d) return "";
  return new Date(y, m - 1, d).toLocaleDateString("en-US", { month: "short", day: "numeric" });
};
</script>

<style scoped>
.panel { border-radius: 16px; height: 100%; }

.panel__title {
  font-size: 14px;
  font-weight: 700;
  color: var(--on-surface);
}

.panel__action { font-weight: 600; font-size: 12px; }

.note {
  background: var(--surface-container-low);
  border: 1px solid var(--line);
  border-radius: 12px;
  padding: 10px 12px;
}

.note__title {
  font-size: 12px;
  font-weight: 700;
  color: var(--on-surface);
}

.note__date {
  font-size: 11px;
  color: var(--outline);
  margin-left: 12px;
  white-space: nowrap;
}

.note__body {
  font-size: 11px;
  color: var(--on-surface-variant);
  margin: 2px 0 0;
}
</style>
