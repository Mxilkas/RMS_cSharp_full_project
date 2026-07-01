import { Clock3, Mail, MapPin, Phone, Send } from 'lucide-react';
import { useState } from 'react';
import ErrorState from '../components/ErrorState.jsx';
import LoadingState from '../components/LoadingState.jsx';
import PageHeading from '../components/PageHeading.jsx';
import { getContactContent, sendContactMessage } from '../api/websiteApi.js';
import useApi from '../hooks/useApi.js';

const initialForm = {
  fullName: '',
  email: '',
  phone: '',
  subject: '',
  message: '',
};

export default function Contact() {
  const { data, loading, error, reload } = useApi(getContactContent, []);
  const [form, setForm] = useState(initialForm);
  const [sending, setSending] = useState(false);
  const [formError, setFormError] = useState('');
  const [success, setSuccess] = useState('');

  if (loading) return <div className="container section-space"><LoadingState /></div>;
  if (error || !data) return <div className="container section-space"><ErrorState message={error} onRetry={reload} /></div>;

  function updateField(event) {
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSending(true);
    setFormError('');
    setSuccess('');

    try {
      const result = await sendContactMessage(form);
      setSuccess(result.message);
      setForm(initialForm);
    } catch (requestError) {
      setFormError(requestError.message);
    } finally {
      setSending(false);
    }
  }

  return (
    <section className="section-space">
      <div className="container">
        <PageHeading
          eyebrow="Contact us"
          title="Let us help you organize your property operations"
          description={data.responseTime}
        />

        <div className="contact-grid">
          <aside className="contact-details-card">
            <h2>{data.companyName}</h2>
            <p>Contact information below is loaded from the Website API endpoint.</p>
            <div className="contact-detail"><MapPin /><div><strong>Office</strong><span>{data.address}</span></div></div>
            <div className="contact-detail"><Phone /><div><strong>Phone</strong><span>{data.phone}</span></div></div>
            <div className="contact-detail"><Mail /><div><strong>Email</strong><span>{data.email}</span></div></div>
            <div className="contact-detail"><Clock3 /><div><strong>Working Hours</strong><span>{data.workingHours}</span></div></div>
          </aside>

          <form className="contact-form-card" onSubmit={handleSubmit}>
            <h2>Send a message</h2>
            <p>The form submits to <code>POST /api/website/contact</code>.</p>

            {success && <div className="alert success-alert">{success}</div>}
            {formError && <div className="alert error-alert">{formError}</div>}

            <div className="form-grid">
              <label><span>Full Name *</span><input name="fullName" value={form.fullName} onChange={updateField} required /></label>
              <label><span>Email *</span><input name="email" type="email" value={form.email} onChange={updateField} required /></label>
              <label><span>Phone</span><input name="phone" value={form.phone} onChange={updateField} /></label>
              <label><span>Subject *</span><input name="subject" value={form.subject} onChange={updateField} required /></label>
              <label className="field-wide"><span>Message *</span><textarea name="message" rows="6" minLength="10" value={form.message} onChange={updateField} required /></label>
            </div>

            <button className="button button-primary" type="submit" disabled={sending}>
              <Send size={17} /> {sending ? 'Sending...' : 'Send Message'}
            </button>
          </form>
        </div>
      </div>
    </section>
  );
}
