<template>
  <a-layout class="layout-root">
    <app-layout-header
      :layout-mode="themeStore.layoutMode"
      :menu-items="menuItems"
      :selected-keys="selectedMenuKeys"
      :online-user-count="onlineUserCount"
      :refreshing="refreshing"
      :username="displayUsername"
      :primary-color="themeStore.primaryColor"
      :title="appTitle"
      @menu-click="handleMenuClick"
      @refresh="handleRefresh"
      @toggle-settings="showSettings = true"
      @user-menu-click="handleUserMenuClick"
      @navigate-online="navigateToOnlineUsers"
    />

    <a-layout class="layout-body">
      <app-layout-sider
        :layout-mode="themeStore.layoutMode"
        :collapsed="collapsed"
        :menu-items="menuItems"
        :selected-keys="selectedMenuKeys"
        :open-keys="openMenuKeys"
        :primary-color="themeStore.primaryColor"
        @update:collapsed="handleCollapse"
        @menu-click="handleMenuClick"
        @open-change="handleOpenChange"
      />

      <a-layout class="content-wrapper">
        <a-layout-content class="layout-content">
          <div class="content-navigation">
            <app-breadcrumb />
            <app-menu-tabs />
          </div>
          <div class="content-view">
            <router-view v-slot="{ Component }">
              <keep-alive :include="cachedViews" :max="10">
                <component :is="Component" />
              </keep-alive>
            </router-view>
          </div>
        </a-layout-content>
      </a-layout>
    </a-layout>

    <!-- 主题设置抽屉 -->
    <theme-settings :visible="showSettings" @update:visible="showSettings = $event" />
  </a-layout>
</template>

<script setup lang="ts">
import { useThemeStore } from '@/stores/modules/common/theme'
import AppBreadcrumb from '@/components/navigation/AppBreadcrumb.vue'
import AppMenuTabs from '@/components/navigation/AppMenuTabs.vue'
import ThemeSettings from '@/components/ThemeSettings.vue'
import AppLayoutHeader from '@/layouts/components/AppLayoutHeader.vue'
import AppLayoutSider from '@/layouts/components/AppLayoutSider.vue'
import { useMainLayout } from '@/layouts/composables/useMainLayout'

const themeStore = useThemeStore()

const {
  collapsed,
  refreshing,
  openMenuKeys,
  showSettings,
  onlineUserCount,
  menuItems,
  selectedMenuKeys,
  cachedViews,
  displayUsername,
  appTitle,
  handleRefresh,
  handleCollapse,
  handleOpenChange,
  handleMenuClick,
  handleUserMenuClick,
  navigateToOnlineUsers,
} = useMainLayout()
</script>

<style scoped>
.layout-root {
  min-height: 100vh;
}

.layout-body {
  min-height: calc(100vh - 64px);
}

.content-wrapper {
  background: #f0f2f5;
}

.layout-content {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 64px);
  overflow: hidden;
}

.content-navigation {
  padding: 8px 24px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.content-view {
  flex: 1;
  padding: 24px;
  box-sizing: border-box;
  overflow-y: auto;
}
</style>
