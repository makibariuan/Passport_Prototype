<script setup>
import { computed } from 'vue';

const props = defineProps(['modelValue', 'disabled']);
const emit = defineEmits(['update:modelValue']);

// Helper to split "08:00" into { hour: 8, minute: "00", period: "AM" }
const timeParts = computed({
  get() {
    if (!props.modelValue) return { hour: '08', minute: '00', period: 'AM' };
    let [h, m] = props.modelValue.split(':');
    let hour = parseInt(h);
    let period = hour >= 12 ? 'PM' : 'AM';
    hour = hour % 12 || 12;
    return {
      hour: hour.toString().padStart(2, '0'),
      minute: m,
      period
    };
  },
  set(val) {
    // Convert back to "HH:mm" 24h format for the backend
    let h = parseInt(val.hour);
    if (val.period === 'PM' && h < 12) h += 12;
    if (val.period === 'AM' && h === 12) h = 0;
    emit('update:modelValue', `${h.toString().padStart(2, '0')}:${val.minute}`);
  }
});

const updatePart = (part, value) => {
  const newParts = { ...timeParts.value, [part]: value };
  timeParts.value = newParts;
};
</script>

<template>
  <div class="time-select-group">
    <select :value="timeParts.hour" @change="updatePart('hour', $event.target.value)" :disabled="disabled">
      <option v-for="h in 12" :key="h" :value="h.toString().padStart(2, '0')">{{ h }}</option>
    </select>
    <select :value="timeParts.minute" @change="updatePart('minute', $event.target.value)" :disabled="disabled">
      <option value="00">00</option>
      <option value="15">15</option>
      <option value="30">30</option>
      <option value="45">45</option>
    </select>
    <select :value="timeParts.period" @change="updatePart('period', $event.target.value)" :disabled="disabled" class="period-select">
      <option value="AM">AM</option>
      <option value="PM">PM</option>
    </select>
  </div>
</template>

<style scoped>
  .time-select-group {
    display: flex;
    gap: 2px;
    align-items: center;
  }

  select {
    padding: 4px 2px;
    border-radius: 4px;
    border: 1px solid #cbd5e1;
    font-size: 0.85rem;
    color: #06195e;
    font-weight: 600;
  }

  .period-select {
    background: #f1f5f9;
  }
</style>
