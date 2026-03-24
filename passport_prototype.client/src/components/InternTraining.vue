<script setup>
  import { ref, computed } from 'vue';

  // 1. DATA STATE
  const newTask = ref("");
  const tasks = ref([
    { id: 1, text: "Setup Vue DevTools", done: true },
    { id: 2, text: "Connect to Makati API", done: false }
  ]);

  // 2. LOGIC (The "Recipe")
  const addTask = () => {
    if (!newTask.value) return;
    tasks.value.push({
      id: Date.now(),
      text: newTask.value,
      done: false
    });
    newTask.value = ""; // Clear the input
  };

  const deleteTask = (id) => {
    tasks.value = tasks.value.filter(t => t.id !== id);
  };

  // 3. COMPUTED (The "Smart Mirror")
  const pendingTasksCount = computed(() => {
    return tasks.value.filter(t => !t.done).length;
  });
</script>

<template>
  <div class="intern-project">
    <h1>Project: Team Activity Hub</h1>

    <div class="stats">
      Pending Tasks: {{ pendingTasksCount }}
    </div>

    <div class="input-group">
      <input v-model="newTask" @keyup.enter="addTask" placeholder="What needs to be done?" />
      <button @click="addTask">Add Task</button>
    </div>

    <ul>
      <li v-for="task in tasks" :key="task.id" :class="{ completed: task.done }">
        <input type="checkbox" v-model="task.done" />
        <span>{{ task.text }}</span>
        <button @click="deleteTask(task.id)" class="delete-btn">×</button>
      </li>
    </ul>
  </div>
</template>

<style scoped>
  /* Encourage them to practice CSS here */
  .completed span {
    text-decoration: line-through;
    color: gray;
  }

  .delete-btn {
    color: red;
    background: none;
    border: none;
    cursor: pointer;
    font-weight: bold;
  }
</style>
