<template>
  <Toast position="top-right" />
  <div class="app-layout" :class="{ 'sidebar-collapsed': auth.isLoggedIn && sidebarCollapsed }">
    <aside class="sidebar" v-if="auth.isLoggedIn">
      <div class="sidebar-header">
        <h2 v-if="!sidebarCollapsed">Popsilencer Stock APP</h2>
        <h2 v-else>PS</h2>
      </div>
      <button class="toggle-btn" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Open menu' : 'Close menu'">
        <i :class="sidebarCollapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"></i>
      </button>
      <nav class="sidebar-nav">
        <router-link to="/products" class="nav-item" active-class="active">
          <i class="pi pi-box"></i>
          <span v-if="!sidebarCollapsed">Products</span>
        </router-link>
        <router-link to="/movements" class="nav-item" active-class="active">
          <i class="pi pi-history"></i>
          <span v-if="!sidebarCollapsed">Movements</span>
        </router-link>
      </nav>
      <div class="sidebar-footer">
        <button class="nav-item logout-btn" @click="auth.logout">
          <i class="pi pi-sign-out"></i>
          <span v-if="!sidebarCollapsed">Logout</span>
        </button>
      </div>
    </aside>
    <main class="main-content" :class="{ 'with-sidebar': auth.isLoggedIn && !sidebarCollapsed }">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from './stores/auth'

const auth = useAuthStore()
const sidebarCollapsed = ref(false)
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: var(--font-family);
}

.app-layout {
  display: flex;
  min-height: 100vh;
  transition: all 0.3s ease;
}

.sidebar {
  width: 240px;
  background: var(--surface-ground);
  border-right: 1px solid var(--surface-border);
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  z-index: 100;
  transition: width 0.3s ease;
  overflow: hidden;
}

.app-layout.sidebar-collapsed .sidebar {
  width: 60px;
}

.sidebar-header {
  padding: 1.5rem;
  border-bottom: 1px solid var(--surface-border);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 64px;
}

.sidebar-header h2 {
  font-size: 1.1rem;
  color: var(--text-color);
  white-space: nowrap;
}

.app-layout.sidebar-collapsed .sidebar-header h2 {
  font-size: 0.9rem;
  color: var(--primary-color);
}

.toggle-btn {
  position: absolute;
  top: 50px;
  right: -14px;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  border: 2px solid var(--surface-border);
  background: var(--surface-card);
  color: var(--text-color-secondary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 101;
  transition: all 0.2s;
  font-size: 0.85rem;
}

.toggle-btn:hover {
  background: var(--primary-color);
  color: var(--primary-contrast-color);
  border-color: var(--primary-color);
}

.sidebar-nav {
  flex: 1;
  padding: 1rem 0;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1.25rem;
  color: var(--text-color-secondary);
  text-decoration: none;
  font-size: 0.9rem;
  transition: all 0.2s;
  border: none;
  background: none;
  width: 100%;
  cursor: pointer;
  text-align: left;
  border-left: 3px solid transparent;
  white-space: nowrap;
}

.nav-item:hover {
  background: var(--surface-hover);
  color: var(--text-color);
}

.nav-item.active {
  background: color-mix(in srgb, var(--primary-color) 15%, transparent);
  color: var(--primary-color);
  border-left-color: var(--primary-color);
  font-weight: 600;
}

.nav-item i {
  font-size: 1.1rem;
  width: 20px;
  text-align: center;
  flex-shrink: 0;
}

.sidebar-footer {
  padding: 1rem 0;
  border-top: 1px solid var(--surface-border);
}

.logout-btn {
  color: var(--text-color-secondary);
}

.logout-btn:hover {
  color: var(--red-500);
  background: var(--surface-hover);
}

.main-content {
  flex: 1;
  padding: 2rem;
}

.main-content.with-sidebar {
  margin-left: 240px;
}

.app-layout.sidebar-collapsed .main-content.with-sidebar {
  margin-left: 60px;
}
</style>
