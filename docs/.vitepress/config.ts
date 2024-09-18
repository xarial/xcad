import { defineConfig } from 'vitepress'
import { generateSidebar } from 'vitepress-sidebar';
import { cleanNavItems } from './clean-nav-items';
const vitepressSidebarOptions = {
    // debugPrint: true,
    documentRootPath: "/docs",
    // resolvePath: "/docs",
    // includeRootIndexFile:true,
    hyphenToSpace: true,
    capitalizeFirst: true,
    includeFolderIndexFile : true,
    // includeEmptyFolder: true,
    collapsed: true,
    excludeFolders:["_assets","_includes","_layouts","_legal","_src", "changelog"],
};

export default defineConfig({    
    head: [['link', { rel: 'icon', href: '/xcad/favicon.ico' }]],
    title: 'XCad',
    description: 'SOLIDWORKS API development made easy',
    srcDir: "./",
    base:"/xcad/",
    themeConfig: {
        logo: '/logo.svg',
        search: {
        provider: 'local'
        },
        nav:[
            { text: 'Contact', link: 'mailto:support@xcad.net' },
            { text: 'Guide', link: 'https://github.com/xarial/xcad-examples' },
            { text:'Changelog', link:'/changelog/'}
        ],
        socialLinks: [
           {icon:'discord', link: 'https://discord.gg/gbhABKu3eJ'} ,
           {icon:'github', link: 'https://github.com/xarial/xcad'} ,
           {icon:'youtube', link: 'https://www.youtube.com/@Xarial'} ,
           {icon:{ svg: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path xmlns="http://www.w3.org/2000/svg" d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 0 1-2.063-2.065 2.064 2.064 0 1 1 2.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></svg>' }, link:'https://www.linkedin.com/company/xarial'},
           {icon:{ svg: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path xmlns="http://www.w3.org/2000/svg" d="M2.204 14.049c-.06.276-.091.56-.091.847 0 3.443 4.402 6.249 9.814 6.249 5.41 0 9.812-2.804 9.812-6.249 0-.274-.029-.546-.082-.809l-.015-.032a.456.456 0 0 1-.029-.165c-.302-1.175-1.117-2.241-2.296-3.103a.422.422 0 0 1-.126-.07c-.026-.02-.045-.042-.067-.064-1.792-1.234-4.356-2.008-7.196-2.008-2.815 0-5.354.759-7.146 1.971a.397.397 0 0 1-.179.124c-1.206.862-2.042 1.937-2.354 3.123a.454.454 0 0 1-.037.171l-.008.015zm9.773 5.441c-1.794 0-3.057-.389-3.863-1.197a.45.45 0 0 1 0-.632.47.47 0 0 1 .635 0c.63.629 1.685.943 3.228.943 1.542 0 2.591-.3 3.219-.929a.463.463 0 0 1 .629 0 .482.482 0 0 1 0 .645c-.809.808-2.065 1.198-3.862 1.198l.014-.028zm-3.606-7.573c-.914 0-1.677.765-1.677 1.677 0 .91.763 1.65 1.677 1.65s1.651-.74 1.651-1.65c0-.912-.739-1.677-1.651-1.677zm7.233 0c-.914 0-1.678.765-1.678 1.677 0 .91.764 1.65 1.678 1.65s1.651-.74 1.651-1.65c0-.912-.739-1.677-1.651-1.677zm4.548-1.595c1.037.833 1.8 1.821 2.189 2.904a1.818 1.818 0 1 0-2.189-2.902v-.002zM2.711 9.963a1.82 1.82 0 0 0-1.173 3.207c.401-1.079 1.172-2.053 2.213-2.876a1.82 1.82 0 0 0-1.039-.329v-.002zm9.217 12.079c-5.906 0-10.709-3.205-10.709-7.142 0-.275.023-.544.068-.809A2.723 2.723 0 0 1 0 11.777a2.725 2.725 0 0 1 2.725-2.713 2.7 2.7 0 0 1 1.797.682c1.856-1.191 4.357-1.941 7.112-1.992l1.812-5.524.404.095s.016 0 .016.002l4.223.993a2.237 2.237 0 0 1 4.296.874c0 1.232-1.003 2.234-2.231 2.234s-2.23-1.004-2.23-2.23l-3.851-.912-1.467 4.477c2.65.105 5.047.854 6.844 2.021a2.663 2.663 0 0 1 1.833-.719 2.716 2.716 0 0 1 2.718 2.711c0 .987-.54 1.886-1.378 2.365.029.255.059.494.059.749-.015 3.938-4.806 7.143-10.72 7.143l-.034.009zm8.179-19.187a1.339 1.339 0 1 0 0 2.678c.732 0 1.33-.6 1.33-1.334 0-.733-.598-1.332-1.347-1.332l.017-.012z"/></svg>' }, link:'https://www.reddit.com/r/xCAD/'},
           {icon:{ svg: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path xmlns="http://www.w3.org/2000/svg" d="M17.67 21.633a3.995 3.995 0 1 1 0-7.99 3.995 3.995 0 0 1 0 7.99m-7.969-9.157a2.497 2.497 0 1 1 0-4.994 2.497 2.497 0 0 1 0 4.994m8.145-7.795h-6.667a6.156 6.156 0 0 0-6.154 6.155v6.667a6.154 6.154 0 0 0 6.154 6.154h6.667A6.154 6.154 0 0 0 24 17.503v-6.667a6.155 6.155 0 0 0-6.154-6.155M3.995 2.339a1.998 1.998 0 1 1-3.996 0 1.998 1.998 0 0 1 3.996 0"/></svg>' }, link:'https://www.nuget.org/profiles/Xarial'},
           {icon:{ svg: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path xmlns="http://www.w3.org/2000/svg" d="M19.199 24C19.199 13.467 10.533 4.8 0 4.8V0c13.165 0 24 10.835 24 24h-4.801zM3.291 17.415c1.814 0 3.293 1.479 3.293 3.295 0 1.813-1.485 3.29-3.301 3.29C1.47 24 0 22.526 0 20.71s1.475-3.294 3.291-3.295zM15.909 24h-4.665c0-6.169-5.075-11.245-11.244-11.245V8.09c8.727 0 15.909 7.184 15.909 15.91z"/></svg>' }, link:'https://xcad.xarial.com/feed.xml'},
           {icon:{ svg: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path xmlns="http://www.w3.org/2000/svg" d="M2.117 3.185C.957 3.185.024 4.243.024 5.557v12.886c0 1.314.933 2.372 2.093 2.372h19.766c1.16 0 2.093-1.058 2.093-2.372V5.557c0-1.314-.933-2.372-2.093-2.372zm18.757 1.587a.811.811 0 0 1 .554 1.407l-7.842 7.46a2.495 2.495 0 0 1-3.53 0l-.002-.002-.004-.002-7.49-7.402A.811.811 0 1 1 3.701 5.08l7.497 7.41.004.002c.279.281.97.27 1.232.006l.008-.008 7.868-7.485a.811.811 0 0 1 .563-.231zm-3.644 7.726a.811.811 0 0 1 .58.253l4.847 4.963a.811.811 0 1 1-1.16 1.133l-4.847-4.963a.811.811 0 0 1 .58-1.386zm-10.176.035a.811.811 0 0 1 .604 1.385l-4.79 4.897A.811.811 0 1 1 1.71 17.68l4.79-4.897a.811.811 0 0 1 .555-.251z"/></svg>' }, link:'info@xarial.com'}
        ],
        sidebar: cleanNavItems(generateSidebar(vitepressSidebarOptions)),
        footer: {
            message: `<a style="margin-left: 10px;margin-right: 10px;" href="/terms-of-use/">Terms Of Use</a>
            <a style="margin-left: 10px;margin-right: 10px;" href="/privacy-policy/">Privacy</a>
            <a style="margin-left: 10px;margin-right: 10px;" href="/cookies-policy/">Cookies</a>`,
            copyright: 'Copyright © 2024 Xarial Pty Limited. All rights reserved.',        
          } 
    }
})

