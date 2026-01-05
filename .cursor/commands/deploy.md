# Деплой приложения

Команда для деплоя приложения.

## Использование

```bash
cd urbanwave-clietn
npm run build
```

## Варианты деплоя

### Vercel
```bash
vercel --prod
```

### Другие платформы
- Netlify: `netlify deploy --prod`
- Docker: `docker build -t urbanwave . && docker push`
- Собственный сервер: `npm run build && scp -r .next user@server:/path`

## Предварительные требования

- Убедитесь, что все изменения закоммичены
- Проверьте, что приложение собирается без ошибок: `npm run build`
- Проверьте переменные окружения на платформе деплоя

