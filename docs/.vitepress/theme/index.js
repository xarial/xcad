import DefaultTheme from 'vitepress/theme'
import YouTube from '../components/YouTube.vue'
import './custom.css'

export default {
    extends: DefaultTheme,
    enhanceApp({ app }) {
        // register your custom global components
        app.component('YouTube', YouTube)
    }
}